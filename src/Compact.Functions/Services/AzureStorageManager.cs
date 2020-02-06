using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;

namespace Compact.Functions
{
    public interface IAzureStorageManager
    {
        Task<string> StoreObject(string containerName, string fileName, object obj);
    }

    public class AzureStorageManager : IAzureStorageManager
    {
        private readonly string _storageConnectionString;

        public AzureStorageManager(string storageConnectionString)
        {
            _storageConnectionString = storageConnectionString;
        }

        public async Task<string> StoreObject(string containerName, string fileName, object obj)
        {
            var storageAccount = ConnectStorageAccount();
            var cloudBlobContainer = FetchBlobContainer(storageAccount, containerName);
            await cloudBlobContainer.CreateIfNotExistsAsync();

            BlobContainerPermissions permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob,
            };

            await cloudBlobContainer.SetPermissionsAsync(permissions);

            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);

            var jsonFileContent = JsonConvert.SerializeObject(obj);

            await cloudBlockBlob.UploadTextAsync(jsonFileContent);

            return cloudBlobContainer.Uri.AbsoluteUri + "/" + fileName;
        }

        public async Task<string> StoreFile(string containerName, string fileName, byte[] fileContent)
        {
            var storageAccount = ConnectStorageAccount();
            var cloudBlobContainer = FetchBlobContainer(storageAccount, containerName);
            await cloudBlobContainer.CreateIfNotExistsAsync();

            BlobContainerPermissions permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob,
            };

            await cloudBlobContainer.SetPermissionsAsync(permissions);

            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);

            await cloudBlockBlob.UploadFromByteArrayAsync(fileContent, 0, fileContent.Length);

            return cloudBlobContainer.Uri.AbsoluteUri + "/" + fileName;
        }

        private CloudStorageAccount ConnectStorageAccount()
        {
            if (CloudStorageAccount.TryParse(_storageConnectionString, out CloudStorageAccount storageAccount))
            {
                return storageAccount;
            }
            else
            {
                throw new WebException("Unable to connect to storage account");
            }
        }

        private static CloudBlobContainer FetchBlobContainer(CloudStorageAccount storageAccount, string containerName)
        {
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
            return cloudBlobClient.GetContainerReference(containerName);
        }
    }
}