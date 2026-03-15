using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MudakAI.Connectors.Azure.Blob;
using MudakAI.Connectors.Discord;

using Settings = MudakAI.Voice.Functions.Settings;

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

        services.AddDiscord(
            new MudakAI.Connectors.Discord.Settings
            {
                DiscordBotToken = settings.DiscordBotToken
            },
            registerEventListener: false);

        services.AddBlobStorage(settings.BlobStorageConnectionString, settings.BlobStorageAudioContainerName);
        services.AddBlobLocks(settings.BlobStorageConnectionString, settings.BlobStorageLocksContainerName);
    })
    .Build();

host.Run();
