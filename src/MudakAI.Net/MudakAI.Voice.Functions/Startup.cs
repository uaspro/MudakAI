using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudakAI.Connectors.Azure.Blob;
using MudakAI.Connectors.Discord;
using System.IO;

[assembly: FunctionsStartup(typeof(MudakAI.Voice.Functions.Startup))]
namespace MudakAI.Voice.Functions
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

            builder.Services.AddDiscord(
                new Connectors.Discord.Settings
                {
                    DiscordBotToken = settings.DiscordBotToken
                },
                registerEventListener: false);

            builder.Services.AddBlobStorage(settings.BlobStorageConnectionString, settings.BlobStorageAudioContainerName);
            builder.Services.AddBlobLocks(settings.BlobStorageConnectionString, settings.BlobStorageLocksContainerName);
        }
    }
}
