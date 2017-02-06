using System.Threading.Tasks;
using Devices.Components;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Turbo.BrickPi.Components.Storage
{
    public class AzureBlobStorageComponent : StorageComponentBase
    {
        CloudStorageAccount storageAccount;
        CloudBlobClient blobClient;
        CloudBlobContainer container;

        public AzureBlobStorageComponent() : base("AzureBlob")
        {
        }

        [Action("Connect")]
        [ActionParameter("ConnectionString")]
        [ActionParameter("StorageAccount", Required = false)]
        [ActionParameter("AccessKey", Required = false)]
        [ActionParameter("ContainerName")]
        [ActionHelp("Connecting to Storage Container in Azure Blob Storage.")]
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
            await ComponentHandler.HandleOutput(data); ;
        }

        [Action("Disconnect")]
        [ActionHelp("Disconnecting from Azure Blob Storage.")]
        protected override async Task DisconnectStorage(MessageContainer data)
        {
            await DisconnectStorage();
            data.AddValue("Disconnect", "Disconnect " + (blobClient != null ? "successful" : "failed"));
            await ComponentHandler.HandleOutput(data); ;
        }

        [Action("List")]
        [Action("ListFiles")]
        [ActionParameter("Path", Required = false)]
        [ActionParameter("FilesOnly", ParameterType = typeof(bool), Required = false)]
        [ActionHelp("List Folders and Files, starting at root or in given path. Set FilesOnly to list only files.")]
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
