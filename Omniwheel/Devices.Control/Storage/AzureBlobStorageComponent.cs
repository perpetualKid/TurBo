using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Base;
using Common.Base.Categories;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Devices.Control.Storage
{
    public class AzureBlobStorageComponent : StorageControllable
    {
        CloudStorageAccount storageAccount;
        CloudBlobClient blobClient;
        CloudBlobContainer container;

        public AzureBlobStorageComponent() : base("AzureBlob")
        {
        }

        protected override async Task ProcessCommand(MessageContainer data)
        {
            switch (data.ResolveParameter(nameof(MessageContainer.FixedPropertyNames.Action), 1).ToUpperInvariant())
            {
                case "HELP":
                    await ComponentHelp(data);
                    break;
                case "CONNECT":
                    await ConnectStorage(data);
                    break;
                case "DISCONNECT":
                    await DisconnectStorage(data);
                    break;
                case "LIST":
                case "LISTFILES":
                    await ListContent(data);
                    break;
            }
        }

        protected override async Task ComponentHelp(MessageContainer data)
        {
            data.AddMultiPartValue("Help", "HELP : Shows this help screen.");
            data.AddMultiPartValue("Help", "CONNECT:<StorageConnectionString>|<StorageAccount>:<AccessKey> : Connecting to Azure Blob Storage.");
            data.AddMultiPartValue("Help", "DISCONNECT: Disconnecting from Azure Blob Storage.");
            data.AddMultiPartValue("Help", "LIST|LISTFILES[:<Path>[:<FilesOnly|True>]] : List Folders and Files or Files only.");
            await HandleOutput(data);
        }

        protected override async Task ConnectStorage(MessageContainer data)
        {
            string containerName;
            string connectionString = data.ResolveParameter("ConnectionString", 2);
            if (CloudStorageAccount.TryParse(connectionString, out storageAccount))
            {
                containerName = data.ResolveParameter("ContainerName", 3); 
                await ConnectStorage(connectionString, containerName);
            }
            else
            {
                string storageAccount = data.ResolveParameter("StorageAccount", 2);
                string accessKey = data.ResolveParameter("AccessKey", 3);
                containerName = data.ResolveParameter("ContainerName", 4);
                await ConnectStorage(storageAccount, accessKey, containerName);
            }
            data.AddValue("Connect", "Connect " + (blobClient != null ? "successful" : "failed"));
            await HandleOutput(data); ;
        }

        protected override async Task DisconnectStorage(MessageContainer data)
        {
            await DisconnectStorage();
            data.AddValue("Disconnect", "Disconnect " + (blobClient != null ? "successful" : "failed"));
            await HandleOutput(data); ;
        }

        protected override async Task ListContent(MessageContainer data)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public async Task ConnectStorage(string storageConnectionString, string containerName, bool createContainer = true)
        {
            containerName = containerName?.ToLowerInvariant();
            //https://github.com/Azure/azure-storage-net/issues/171
            if (null == storageAccount)
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            container = blobClient.GetContainerReference(containerName);
            if (createContainer && !await container.ExistsAsync())
            {
                // Create the container if it doesn't already exist.
                await container.CreateAsync();
                await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });
            }
        }

        public async Task ConnectStorage(string blobStorageAccount, string accessKey, string containerName, bool createContainer = true)
        {
            containerName = containerName?.ToLowerInvariant();
            //https://github.com/Azure/azure-storage-net/issues/171
            storageAccount = new CloudStorageAccount(new StorageCredentials(blobStorageAccount, accessKey), true);
            blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            container = blobClient.GetContainerReference(containerName);
            if (createContainer && !await container.ExistsAsync())
            {
                // Create the container if it doesn't already exist.
                await container.CreateAsync();
                await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });
            }

        }

        public async Task DisconnectStorage()
        {
            if (blobClient != null)
            {
                blobClient = null;
                storageAccount = null;
            }
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }

}
