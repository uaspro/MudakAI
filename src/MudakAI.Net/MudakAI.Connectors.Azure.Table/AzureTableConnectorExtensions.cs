using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;

namespace MudakAI.Connectors.Azure.Table
{
    public static class AzureTableConnectorExtensions
    {
        public static IServiceCollection AddTableStorage(this IServiceCollection services, string connectionString)
        {
            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddTableServiceClient(connectionString);
            });

            return services;
        }
    }
}