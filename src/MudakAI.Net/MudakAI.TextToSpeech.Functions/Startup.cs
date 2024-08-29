using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudakAI.Connectors.Azure.Blob;
using MudakAI.Connectors.OpenAI;
using MudakAI.TextToSpeech.Functions.Services;
using System.IO;

[assembly: FunctionsStartup(typeof(MudakAI.TextToSpeech.Functions.Startup))]
namespace MudakAI.TextToSpeech.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            var settings = Settings.CreateFrom(configuration);

            builder.Services.AddSingleton(settings);

            builder.Services.AddDaprClient();

            builder.Services.AddBlobStorage(settings.BlobStorageConnectionString, settings.BlobStorageAudioContainerName);

            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddServiceBusClient(settings.ServiceBusSendConnectionString);

                clientBuilder.AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) =>
                        provider.GetService<ServiceBusClient>()
                                .CreateSender(settings.ServiceBusPlaybackQueueName))
                    .WithName(settings.ServiceBusPlaybackQueueName);
            });

            builder.Services.AddOpenAITTS(settings.OpenAI);
            builder.Services.AddTransient<SpeechGenerationApiService>();
        }
    }
}
