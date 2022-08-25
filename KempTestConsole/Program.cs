using System;
using System.Collections.Generic;
using Keyfactor.Extensions.Orchestrator.Kemp.Jobs;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Orchestrators.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KempTestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 0) return;
            // Display message to user to provide parameters.
            Console.WriteLine("Please enter parameter values.  Inventory or Management");
            var input = Console.ReadLine();
            Console.WriteLine("Enter Client Machine Json File Name freshcertman-9ff09dc4ff67.json");
            var clientMachine = Console.ReadLine();
            Console.WriteLine("Enter Google Cloud Store Path freshcertman");
            var storePath = Console.ReadLine();

            switch (input)
            {
                case "Inventory":
                    ILoggerFactory invLoggerFactory = new LoggerFactory();
                    var invLogger = invLoggerFactory.CreateLogger<Inventory>();

                    var inv = new Inventory(invLogger);

                    var invJobConfig = GetInventoryJobConfiguration(clientMachine, storePath);

                    SubmitInventoryUpdate sui = GetItems;
                    inv.ProcessJob(invJobConfig, sui);
                    Console.Write("Successful Inventory!");
                    break;
                case "Management":
                    Console.WriteLine("Select Management Type Add or Remove");
                    var mgmtType = Console.ReadLine();
                    Console.WriteLine("Overwrite? Enter true or false");
                    var overWrite = Console.ReadLine();
                    Console.WriteLine("Alias Enter Alias Name");
                    var alias = Console.ReadLine();
                    Console.WriteLine("Enter Location global or another region");
                    var location = Console.ReadLine();
                    Console.WriteLine("Enter Google Cloud Project Number 210777491775");
                    var projectNumber = Console.ReadLine();

                    if (mgmtType == "Add")
                    {
                        ILoggerFactory loggerFactory = new LoggerFactory();
                        var logger = loggerFactory.CreateLogger<Management>();

                        var mgmt = new Management(logger);

                        var jobConfiguration = GetJobConfiguration(clientMachine, storePath, projectNumber,
                            overWrite,
                            alias, location);

                        var result = mgmt.ProcessJob(jobConfiguration);

                        if (result.Result == OrchestratorJobStatusJobResult.Success) Console.Write("Success");
                    }

                    if (mgmtType == "Remove")
                    {
                        ILoggerFactory loggerFactory = new LoggerFactory();
                        var logger = loggerFactory.CreateLogger<Management>();

                        var mgmt = new Management(logger);
                        var jobConfig = GetRemoveJobConfiguration(clientMachine, storePath, projectNumber,
                            overWrite, alias, location);
                        var result = mgmt.ProcessJob(jobConfig);

                        if (result.Result == OrchestratorJobStatusJobResult.Success) Console.Write("Success");
                    }

                    break;
            }
        }


        public static bool GetItems(IEnumerable<CurrentInventoryItem> items)
        {
            return true;
        }

        public static ManagementJobConfiguration GetJobConfiguration(string clientMachineJson, string storePath,
            string projectNumber, string overWrite,
            string alias, string location)
        {
            var privateKeyConfig =
                $"{{\"LastInventory\":[],\"CertificateStoreDetails\":{{\"ClientMachine\":\"{clientMachineJson}\",\"StorePath\":\"{storePath}\",\"StorePassword\":null,\"Properties\":\"{{\\\"Location\\\":\\\"{location}\\\",\\\"Project Number\\\":\\\"{projectNumber}\\\"}}\",\"Type\":6110}},\"OperationType\":2,\"Overwrite\":{overWrite},\"JobCertificate\":{{\"Thumbprint\":null,\"Contents\":\"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\",\"Alias\":\"{alias}\",\"PrivateKeyPassword\":\"zsvkW2cyfc4w\"}},\"JobCancelled\":false,\"ServerError\":null,\"JobHistoryId\":362893,\"RequestStatus\":1,\"ServerUsername\":null,\"ServerPassword\":null,\"UseSSL\":true,\"JobProperties\":{{}},\"JobTypeId\":\"00000000-0000-0000-0000-000000000000\",\"JobId\":\"6f4f5268-bf51-4ca0-870b-94c3c8a82fe3\",\"Capability\":\"CertStores.GcpCertManager.Management\"}}";

            var jobConfigString = privateKeyConfig;

            var result = JsonConvert.DeserializeObject<ManagementJobConfiguration>(jobConfigString);
            return result;
        }

        public static InventoryJobConfiguration GetInventoryJobConfiguration(string clientMachineJson, string storePath)
        {
            var jobConfigString =
                "{\"LastInventory\":[{\"Alias\":\"cert12\",\"PrivateKeyEntry\":true,\"Thumbprints\":[\"8C70C6EFBDDB09627DFECC6761B46CC6EF0C578D\"]},{\"Alias\":\"cert20\",\"PrivateKeyEntry\":true,\"Thumbprints\":[\"C97D508D9192872D7349A79F7AEE13201443F844\"]},{\"Alias\":\"cert27\",\"PrivateKeyEntry\":true,\"Thumbprints\":[\"7FE8827A8626307BD635D50EF8EE2A64D91DF9F7\"]},{\"Alias\":\"map30/me30/cert30\",\"PrivateKeyEntry\":true,\"Thumbprints\":[\"480E773F01F188E3731D6E921ED83E53C0D1F08D\"]}],\"CertificateStoreDetails\":{\"ClientMachine\":\"" +
                clientMachineJson + "\",\"StorePath\":\"" + storePath +
                "\",\"StorePassword\":\"\",\"Properties\":\"{\\\"Location\\\":\\\"global\\\",\\\"Project Number\\\":\\\"210777491775\\\"}\",\"Type\":6110},\"JobCancelled\":false,\"ServerError\":null,\"JobHistoryId\":362890,\"RequestStatus\":1,\"ServerUsername\":null,\"ServerPassword\":null,\"UseSSL\":true,\"JobProperties\":null,\"JobTypeId\":\"00000000-0000-0000-0000-000000000000\",\"JobId\":\"e131894b-1bbf-4743-8a01-534dc89d409b\",\"Capability\":\"CertStores.GcpCertManager.Inventory\"}";
            var result = JsonConvert.DeserializeObject<InventoryJobConfiguration>(jobConfigString);
            return result;
        }

        public static ManagementJobConfiguration GetRemoveJobConfiguration(string clientMachineJson, string storePath,
            string projectNumber, string overWrite,
            string alias, string location)
        {
            var jobConfigString =
                $"{{\"LastInventory\":[],\"CertificateStoreDetails\":{{\"ClientMachine\":\"{clientMachineJson}\",\"StorePath\":\"{storePath}\",\"StorePassword\":null,\"Properties\":\"{{\\\"Location\\\":\\\"{location}\\\",\\\"Project Number\\\":\\\"{projectNumber}\\\"}}\",\"Type\":6110}},\"OperationType\":3,\"Overwrite\":{overWrite},\"JobCertificate\":{{\"Thumbprint\":null,\"Contents\":\"\",\"Alias\":\"{alias}\",\"PrivateKeyPassword\":null}},\"JobCancelled\":false,\"ServerError\":null,\"JobHistoryId\":362897,\"RequestStatus\":1,\"ServerUsername\":null,\"ServerPassword\":null,\"UseSSL\":true,\"JobProperties\":{{}},\"JobTypeId\":\"00000000-0000-0000-0000-000000000000\",\"JobId\":\"bcc9031a-8c37-4def-8de9-3bafe4afd0c1\",\"Capability\":\"CertStores.GcpCertManager.Management\"}}";
            var result = JsonConvert.DeserializeObject<ManagementJobConfiguration>(jobConfigString);
            return result;
        }
    }
}