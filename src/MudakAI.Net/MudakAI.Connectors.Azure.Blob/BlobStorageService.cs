using Azure.Storage.Blobs;
using System.IO;
using System.Threading.Tasks;

namespace MudakAI.Connectors.Azure.Blob
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainerClient;

        public BlobStorageService(string containerName, BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;

            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task Upload(string fileName, Stream fileStream)
        {
            await _blobContainerClient.CreateIfNotExistsAsync();

            await _blobContainerClient.UploadBlobAsync(fileName, fileStream);
        }

        public async Task<Stream> Get(string fileName)
        {
            var blobClient = _blobContainerClient.GetBlobClient(fileName);

            var blobDownloaResult = await blobClient.DownloadStreamingAsync();

            return blobDownloaResult.Value.Content;
        }

        public async Task Delete(string fileName)
        {
            var blobClient = _blobContainerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}
