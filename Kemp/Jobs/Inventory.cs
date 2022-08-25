using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Keyfactor.Extensions.Orchestrator.Kemp.Client;
using Keyfactor.Extensions.Orchestrator.Kemp.Client.Models;
using Keyfactor.Logging;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Orchestrators.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Keyfactor.Extensions.Orchestrator.Kemp.Jobs
{
    public class Inventory : IInventoryJobExtension
    {
        private readonly ILogger<Inventory> _logger;

        public Inventory(ILogger<Inventory> logger)
        {
            _logger = logger;
        }

        public string ExtensionName => "Kemp";

        public JobResult ProcessJob(InventoryJobConfiguration jobConfiguration,
            SubmitInventoryUpdate submitInventoryUpdate)
        {
            try
            {
                _logger.MethodEntry();
                return PerformInventory(jobConfiguration, submitInventoryUpdate);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occured in Inventory.ProcessJob: {LogHandler.FlattenException(e)}");
                throw;
            }
        }

        private JobResult PerformInventory(InventoryJobConfiguration config, SubmitInventoryUpdate submitInventory)
        {
            try
            {
                _logger.MethodEntry(LogLevel.Debug);
                _logger.LogTrace($"Inventory Config {JsonConvert.SerializeObject(config)}");
                _logger.LogTrace(
                    $"Client Machine: {config.CertificateStoreDetails.ClientMachine} ApiKey: {config.ServerPassword}");

                var client = new KempClient(config);

                var certificatesResult = client.GetCertificates().Result;
                var intermediateCertificates = client.GetIntermediateCertificates().Result;

                //Debug Write Certificate List Response from Palo Alto
                var listWriter = new StringWriter();
                var listSerializer = new XmlSerializer(typeof(CertListResponse));
                listSerializer.Serialize(listWriter, certificatesResult);
                _logger.LogTrace($"Certificate List Result {listWriter}");

                //Debug Write Intermediate Certificate List Response from Palo Alto
                var intListWriter = new StringWriter();
                var intListSerializer = new XmlSerializer(typeof(CertListResponse));
                intListSerializer.Serialize(intListWriter, intermediateCertificates);
                _logger.LogTrace($"Intermediate List Result {intListWriter}");

                var inventoryItems = (from cert in certificatesResult.Success.Data.Certs
                    let certPem = client.GetCertificate(cert.Name).Result
                    select BuildInventoryItem(cert.Name, certPem.Success.Data.Certificate, true)).ToList();
                inventoryItems.AddRange(from cert in certificatesResult.Success.Data.Certs
                    let certPem = client.GetIntermediateCertificate(cert.Name).Result
                    select BuildInventoryItem(cert.Name, certPem.Success.Data.Certificate, false));

                _logger.LogTrace("Submitting Inventory To Keyfactor via submitInventory.Invoke");
                submitInventory.Invoke(inventoryItems);
                _logger.LogTrace("Submitted Inventory To Keyfactor via submitInventory.Invoke");

                _logger.MethodExit(LogLevel.Debug);

                _logger.LogTrace("Return Success");
                return new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Success,
                    JobHistoryId = config.JobHistoryId,
                    FailureMessage = ""
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"PerformInventory Error: {LogHandler.FlattenException(e)}");
                throw;
            }
        }

        protected virtual CurrentInventoryItem BuildInventoryItem(string alias, string certPem, bool privateKey)
        {
            try
            {
                _logger.MethodEntry();
                _logger.LogTrace($"Alias: {alias} Pem: {certPem} PrivateKey: {privateKey}");


                var acsi = new CurrentInventoryItem
                {
                    Alias = alias,
                    Certificates = new[] {certPem},
                    ItemStatus = OrchestratorInventoryItemStatus.Unknown,
                    PrivateKeyEntry = privateKey,
                    UseChainLevel = false
                };

                _logger.MethodExit();
                return acsi;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred in Inventory.BuildInventoryItem: {LogHandler.FlattenException(e)}");
                throw;
            }
        }
    }
}