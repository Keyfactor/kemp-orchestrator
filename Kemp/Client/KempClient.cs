using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Keyfactor.Extensions.Orchestrator.Kemp.Client.Models;
using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;
using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.Kemp.Client
{
    public class KempClient
    {

        public KempClient(InventoryJobConfiguration config)
        {
            try
            {
                Logger = LogHandler.GetClassLogger<KempClient>();
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
                };
                HttpClient = new HttpClient(httpClientHandler)
                    {BaseAddress = new Uri("https://" + config.CertificateStoreDetails.ClientMachine)};
                ApiKey = config.ServerPassword;
            }
            catch (Exception e)
            {
                Logger.LogError("Error in Constructor KempClient(InventoryJobConfiguration config): " +
                                LogHandler.FlattenException(e));
                throw;
            }
        }

        public KempClient(ManagementJobConfiguration config)
        {
            try
            {
                Logger = LogHandler.GetClassLogger<KempClient>();
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
                };
                HttpClient = new HttpClient(httpClientHandler)
                    {BaseAddress = new Uri("https://" + config.CertificateStoreDetails.ClientMachine)};
                ApiKey = config.ServerPassword;
            }
            catch (Exception e)
            {
                Logger.LogError("Error in Constructor KempClient(ManagementJobConfiguration config): " +
                                LogHandler.FlattenException(e));
                throw;
            }
        }

        private ILogger Logger { get; }

        private string ApiKey { get; }

        private HttpClient HttpClient { get; }

        public async Task<CertListResponse> GetIntermediateCertificates()
        {
            try
            {
                Logger.MethodEntry();
                var uri = $"/access/listintermediate?apikey={ApiKey}";
                var response = await GetXmlResponseAsync<CertListResponse>(await HttpClient.GetAsync(uri));
                Logger.MethodExit();
                return response;
            }
            catch (Exception e)
            {
                Logger.LogError(
                    $"Error Occured in KempClient.GetIntermediateCertificates: {LogHandler.FlattenException(e)}");
                throw;
            }
        }

        public async Task<CertResponse> GetIntermediateCertificate(string alias)
        {
            try
            {
                Logger.MethodEntry();
                var uri = $"/access/readintermediate?apikey={ApiKey}&cert={alias}";
                var response = await GetXmlResponseAsync<CertResponse>(await HttpClient.GetAsync(uri));
                Logger.MethodExit();
                return response;
            }
            catch (Exception e)
            {
                Logger.LogError(
                    $"Error Occured in KempClient.GetIntermediateCertificate: {LogHandler.FlattenException(e)}");
                throw;
            }
        }

        public async Task<CertListResponse> GetCertificates()
        {
            try
            {
                Logger.MethodEntry();
                var uri = $"/access/listcert?apikey={ApiKey}";
                var response = await GetXmlResponseAsync<CertListResponse>(await HttpClient.GetAsync(uri));
                Logger.MethodExit();
                return response;
            }
            catch (Exception e)
            {
                Logger.LogError($"Error Occured in KempClient.GetCertificates: {LogHandler.FlattenException(e)}");
                throw;
            }
        }

        public async Task<CertResponse> GetCertificate(string alias)
        {
            try
            {
                Logger.MethodEntry();
                var uri = $"/access/readcert?apikey={ApiKey}&cert={alias}";
                var response = await GetXmlResponseAsync<CertResponse>(await HttpClient.GetAsync(uri));
                Logger.MethodExit();
                return response;
            }
            catch (Exception e)
            {
                Logger.LogError(
                    $"Error Occured in KempClient.GetIntermediateCertificate: {LogHandler.FlattenException(e)}");
                throw;
            }
        }

        public async Task<CertResponse> ReplaceCertificate(string alias, string content, int overWriteFlag,
            bool hasPrivateKey)
        {
            try
            {
                Logger.MethodEntry();
                var certType = "addintermediate";
                if (hasPrivateKey)
                    certType = "addcert";

                var uri = $"/access/{certType}?apikey={ApiKey}&cert={alias}&replace={overWriteFlag}";

                HttpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/x-pkcs12"));
                var byteContent = Encoding.ASCII.GetBytes(content);
                HttpContent byteArrayContent = new ByteArrayContent(byteContent);
                var response =
                    await GetXmlResponseAsync<CertResponse>(await HttpClient.PostAsync(uri, byteArrayContent));
                Logger.MethodExit();
                return response;
            }
            catch (Exception e)
            {
                Logger.LogError(
                    $"Error Occured in KempClient.GetIntermediateCertificate: {LogHandler.FlattenException(e)}");
                throw;
            }
        }

        public async Task<CertResponse> DeleteCertificate(string alias)
        {
            try
            {
                Logger.MethodEntry();
                var uri = $"/access/delcert?apikey={ApiKey}&cert={alias}";
                var response = await GetXmlResponseAsync<CertResponse>(await HttpClient.GetAsync(uri));
                Logger.MethodExit();
                return response;
            }
            catch (Exception e)
            {
                Logger.LogError($"Error Occured in KempClient.DeleteCertificate: {LogHandler.FlattenException(e)}");
                throw;
            }
        }

        public async Task<CertResponse> DeleteIntermediateCertificate(string alias)
        {
            try
            {
                Logger.MethodEntry();
                var uri = $"/access/delintermediate?apikey={ApiKey}&cert={alias}";
                var response = await GetXmlResponseAsync<CertResponse>(await HttpClient.GetAsync(uri));
                Logger.MethodExit();
                return response;
            }
            catch (Exception e)
            {
                Logger.LogError(
                    $"Error Occured in KempClient.DeleteIntermediateCertificate: {LogHandler.FlattenException(e)}");
                throw;
            }
        }

        private async Task<T> GetXmlResponseAsync<T>(HttpResponseMessage response)
        {
            try
            {
                EnsureSuccessfulResponse(response);
                var stringResponse =
                    await new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEndAsync();
                var serializer =
                    new XmlSerializer(typeof(T));
                var xmlReader = XmlReader.Create(new StringReader(stringResponse));
                return (T) serializer.Deserialize(xmlReader);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error Occured in KempClient.GetXmlResponseAsync: {e.Message}");
                throw;
            }
        }

        private void EnsureSuccessfulResponse(HttpResponseMessage response)
        {
            try
            {
                if (!response.IsSuccessStatusCode)
                {
                    var error = new StreamReader(response.Content.ReadAsStreamAsync().Result).ReadToEnd();
                    throw new Exception($"Request to Kemp was not successful - {response.StatusCode} - {error}");
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Error Occured in KempClient.EnsureSuccessfulResponse: {e.Message}");
                throw;
            }
        }
    }
}