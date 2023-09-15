using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;

namespace MudakAI.Connectors.Azure.Blob
{
    public static class AzureBlobConnectorExtensions
    {
        public static IServiceCollection AddBlobStorage(this IServiceCollection services, string connectionString, string containerName)
        {
            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddBlobServiceClient(connectionString);
            });

            services.AddTransient(
                sp => new BlobStorageService(containerName, sp.GetRequiredService<BlobServiceClient>()));

            return services;
        }
        public static IServiceCollection AddBlobLocks(this IServiceCollection services, string connectionString, string containerName)
        {
            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddBlobServiceClient(connectionString);
            });

            services.AddTransient(
                sp => new BlobLocksService(containerName, sp.GetRequiredService<BlobServiceClient>()));

            return services;
        }
    }
}