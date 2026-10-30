using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudakAI.Connectors.Azure.Blob;
using MudakAI.Connectors.OpenAI;
using MudakAI.TextToSpeech.Functions.Services;

using Settings = MudakAI.TextToSpeech.Functions.Settings;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        config
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        var settings = Settings.CreateFrom(configuration);

        services.AddSingleton(settings);

        services.AddBlobStorage(settings.BlobStorageConnectionString, settings.BlobStorageAudioContainerName);

        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddServiceBusClient(settings.ServiceBusSendConnectionString);

            clientBuilder.AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) =>
                    provider.GetService<ServiceBusClient>()
                            .CreateSender(settings.ServiceBusPlaybackQueueName))
                .WithName(settings.ServiceBusPlaybackQueueName);
        });

        services.AddOpenAITTS(settings.OpenAI);
        services.AddTransient<SpeechGenerationApiService>();
    })
    .Build();

host.Run();
