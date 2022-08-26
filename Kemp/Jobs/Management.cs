using System;
using System.IO;
using System.Linq;
using System.Text;
using Keyfactor.Extensions.Orchestrator.Kemp.Client;
using Keyfactor.Extensions.Orchestrator.Kemp.Client.Models;
using Keyfactor.Logging;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Orchestrators.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;

namespace Keyfactor.Extensions.Orchestrator.Kemp.Jobs
{
    public class Management : IManagementJobExtension
    {
        private static readonly string certStart = "-----BEGIN CERTIFICATE-----\n";
        private static readonly string certEnd = "\n-----END CERTIFICATE-----";

        private static readonly Func<string, string> Pemify = ss =>
            ss.Length <= 64 ? ss : ss.Substring(0, 64) + "\n" + Pemify(ss.Substring(64));

        private readonly ILogger<Management> _logger;

        public Management(ILogger<Management> logger)
        {
            _logger = logger;
        }

        protected internal virtual AsymmetricKeyEntry KeyEntry { get; set; }

        public string ExtensionName => "Kemp";


        public JobResult ProcessJob(ManagementJobConfiguration jobConfiguration)
        {
            try
            {
                _logger.MethodEntry();
                _logger.MethodExit();
                return PerformManagement(jobConfiguration);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred in Management.ProcessJob: {LogHandler.FlattenException(e)}");
                throw;
            }
        }

        private JobResult PerformManagement(ManagementJobConfiguration config)
        {
            try
            {
                _logger.MethodEntry();
                var complete = new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Failure,
                    JobHistoryId = config.JobHistoryId,
                    FailureMessage =
                        "Invalid Management Operation"
                };

                if (config.OperationType.ToString() == "Add")
                {
                    _logger.LogTrace("Adding...");
                    _logger.LogTrace($"Add Config Json {JsonConvert.SerializeObject(config)}");
                    complete = PerformAddition(config);
                }
                else if (config.OperationType.ToString() == "Remove")
                {
                    _logger.LogTrace("Removing...");
                    _logger.LogTrace($"Remove Config Json {JsonConvert.SerializeObject(config)}");
                    complete = PerformRemoval(config);
                }

                _logger.MethodExit();
                return complete;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred in Management.PerformManagement: {LogHandler.FlattenException(e)}");
                throw;
            }
        }


        private JobResult PerformRemoval(ManagementJobConfiguration config)
        {
            try
            {
                _logger.MethodEntry();

                _logger.LogTrace(
                    $"Credentials JSON: Url: {config.CertificateStoreDetails.ClientMachine} Password: {config.ServerPassword}");


                var client = new KempClient(config);
                _logger.LogTrace("Getting Credentials from Google...");
                _ = client.DeleteCertificate(config.JobCertificate.Alias).Result;
                _logger.LogTrace("Got Credentials from Google");


                _logger.MethodExit();

                return new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Success,
                    JobHistoryId = config.JobHistoryId,
                    FailureMessage = ""
                };
            }
            catch (Exception e)
            {
                return new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Failure,
                    JobHistoryId = config.JobHistoryId,
                    FailureMessage = $"PerformRemoval: {LogHandler.FlattenException(e)}"
                };
            }
        }


        private JobResult PerformAddition(ManagementJobConfiguration config)
        {
            //Temporarily only performing additions
            try
            {
                _logger.MethodEntry();
                _logger.LogTrace(
                    $"Credentials JSON: Url: {config.CertificateStoreDetails.ClientMachine} Password: {config.ServerPassword}");


                var client = new KempClient(config);
                var hasPrivateKey = !string.IsNullOrWhiteSpace(config.JobCertificate.PrivateKeyPassword);

                var duplicate =
                    CheckForDuplicate(config.JobCertificate.Alias, hasPrivateKey, client);
                _logger.LogTrace($"Duplicate? = {duplicate}");

                //Check for Duplicate already in Google Certificate Manager, if there, make sure the Overwrite flag is checked before replacing
                if (duplicate && config.Overwrite || !duplicate)
                {
                    _logger.LogTrace("Either not a duplicate or overwrite was chosen....");
                    if (hasPrivateKey) // This is a PFX Entry
                    {
                        _logger.LogTrace($"Found Private Key {config.JobCertificate.PrivateKeyPassword}");

                        if (string.IsNullOrWhiteSpace(config.JobCertificate.Alias))
                            _logger.LogTrace("No Alias Found");

                        // Load PFX
                        var pfxBytes = Convert.FromBase64String(config.JobCertificate.Contents);
                        Pkcs12Store p;
                        using (var pfxBytesMemoryStream = new MemoryStream(pfxBytes))
                        {
                            p = new Pkcs12Store(pfxBytesMemoryStream,
                                config.JobCertificate.PrivateKeyPassword.ToCharArray());
                        }

                        _logger.LogTrace(
                            $"Created Pkcs12Store containing Alias {config.JobCertificate.Alias} Contains Alias is {p.ContainsAlias(config.JobCertificate.Alias)}");

                        // Extract private key
                        string alias;
                        string privateKeyString;
                        using (var memoryStream = new MemoryStream())
                        {
                            using (TextWriter streamWriter = new StreamWriter(memoryStream))
                            {
                                _logger.LogTrace("Extracting Private Key...");
                                var pemWriter = new PemWriter(streamWriter);
                                _logger.LogTrace("Created pemWriter...");
                                alias = p.Aliases.Cast<string>().SingleOrDefault(a => p.IsKeyEntry(a));
                                _logger.LogTrace($"Alias = {alias}");
                                var publicKey = p.GetCertificate(alias).Certificate.GetPublicKey();
                                _logger.LogTrace($"publicKey = {publicKey}");
                                KeyEntry = p.GetKey(alias);
                                _logger.LogTrace($"KeyEntry = {KeyEntry}");
                                if (KeyEntry == null) throw new Exception("Unable to retrieve private key");

                                var privateKey = KeyEntry.Key;
                                _logger.LogTrace($"privateKey = {privateKey}");
                                var keyPair = new AsymmetricCipherKeyPair(publicKey, privateKey);

                                pemWriter.WriteObject(keyPair.Private);
                                streamWriter.Flush();
                                privateKeyString = Encoding.ASCII.GetString(memoryStream.GetBuffer()).Trim()
                                    .Replace("\r", "").Replace("\0", "");
                                _logger.LogTrace($"Got Private Key String {privateKeyString}");
                                memoryStream.Close();
                                streamWriter.Close();
                                _logger.LogTrace("Finished Extracting Private Key...");
                            }
                        }

                        var pubCertPem =
                            Pemify(Convert.ToBase64String(p.GetCertificate(alias).Certificate.GetEncoded()));
                        _logger.LogTrace($"Public cert Pem {pubCertPem}");

                        var certPem = privateKeyString + certStart + pubCertPem + certEnd;

                        _logger.LogTrace($"Got certPem {certPem}");
                        pubCertPem = $"-----BEGIN CERTIFICATE-----\r\n{pubCertPem}\r\n-----END CERTIFICATE-----";
                        _logger.LogTrace($"Public Cert Pem: {pubCertPem}");

                        var replaceCertificateResponse =
                            ReplaceCertificate(config.JobCertificate.Alias, certPem, config.Overwrite, client, true);

                        _logger.LogTrace($"Replace Response Code: {replaceCertificateResponse.Code}");

                        //5. Return success from job
                        return new JobResult
                        {
                            Result = OrchestratorJobStatusJobResult.Success,
                            JobHistoryId = config.JobHistoryId,
                            FailureMessage = ""
                        };
                    }
                }


                _logger.MethodExit();
                return new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Failure,
                    JobHistoryId = config.JobHistoryId,
                    FailureMessage =
                        $"Duplicate alias {config.JobCertificate.Alias} found in the Kemp Load Balancer, to overwrite use the overwrite flag."
                };
            }
            catch (Exception e)
            {
                return new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Failure,
                    JobHistoryId = config.JobHistoryId,
                    FailureMessage = $"Management/Add {LogHandler.FlattenException(e)}"
                };
            }
        }

        private CertResponse ReplaceCertificate(string alias, string content, bool overwrite, KempClient client,
            bool hasPrivateKey)
        {
            try
            {
                _logger.MethodEntry();
                var overWriteFlg = 0;
                if (overwrite)
                    overWriteFlg = 1;

                var certResponse = client.ReplaceCertificate(alias, content, overWriteFlg, hasPrivateKey).Result;

                _logger.LogTrace($"Response Code : {certResponse.Code}");

                _logger.MethodExit();
                return certResponse;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occured in Management.ReplaceCertificate: {LogHandler.FlattenException(e)}");
                throw;
            }
        }


        private bool CheckForDuplicate(string alias, bool hasPrivateKey, KempClient client)
        {
            try
            {
                _logger.MethodEntry();
                var certResponse = hasPrivateKey
                    ? client.GetCertificates().Result
                    : client.GetIntermediateCertificates().Result;
                _logger.MethodExit();
                return certResponse.Success.Data.Certs.Count(p => p.Name == alias) == 1;
            }
            catch (Exception e)
            {
                _logger.LogError(
                    $"Error Checking for Duplicate Cert in Management.CheckForDuplicate {LogHandler.FlattenException(e)}");
                throw;
            }
        }
    }
}