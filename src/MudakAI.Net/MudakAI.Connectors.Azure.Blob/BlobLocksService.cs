using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MudakAI.Connectors.Azure.Blob
{
    public class BlobLocksService
    {
        private const int WaitForUnlockDelayMilliseconds = 500;

        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainerClient;

        public BlobLocksService(string containerName, BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;

            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task AcquireLock(string lockId, string keyId, TimeSpan leaseDuration)
        {
            await _blobContainerClient.CreateIfNotExistsAsync();

            var blobClient = _blobContainerClient.GetBlobClient(lockId);
            if (!await blobClient.ExistsAsync())
            {
                try
                {
                    await blobClient.UploadAsync(Stream.Null);
                }
                catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
                {
                    // ignored
                }
            }

            await WaitForLockReleaseIfExists(lockId);

            var lockAquired = false;
            while (!lockAquired)
            {
                try
                {
                    var leaseId = GetLeaseId(keyId);
                    var leaseClient = blobClient.GetBlobLeaseClient(leaseId);
                    await leaseClient.AcquireAsync(leaseDuration);

                    lockAquired = true;
                }
                catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
                {
                    await WaitForLockReleaseIfExists(lockId);
                }
            }
        }

        public async Task ReleaseLock(string lockId, string keyId)
        {
            var blobClient = _blobContainerClient.GetBlobClient(lockId);
            if (!await blobClient.ExistsAsync())
            {
                return;
            }

            var leaseId = GetLeaseId(keyId);
            var leaseClient = blobClient.GetBlobLeaseClient(leaseId);
            await leaseClient.ReleaseAsync();
        }

        private async Task WaitForLockReleaseIfExists(string lockId)
        {
            var blobClient = _blobContainerClient.GetBlobClient(lockId);

            do
            {
                var blobPropertiesResponse = await blobClient.GetPropertiesAsync();
                var blobProperties = blobPropertiesResponse.Value;

                if (blobProperties.LeaseStatus != LeaseStatus.Locked)
                {
                    break;
                }

                await Task.Delay(WaitForUnlockDelayMilliseconds);
            }
            while (true);
        }

        private static string GetLeaseId(string input)
        {
            Guid leaseIdGuid;
            if (ulong.TryParse(input, out var inputAsULong))
            {
                var bytes = new byte[16];
                BitConverter.GetBytes(inputAsULong).CopyTo(bytes, 8);
                leaseIdGuid = new Guid(bytes);
            }
            else
            {
                using var md5 = MD5.Create();
                var hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                leaseIdGuid = new Guid(hash);
            }

            return leaseIdGuid.ToString("N");
        }
    }
}
