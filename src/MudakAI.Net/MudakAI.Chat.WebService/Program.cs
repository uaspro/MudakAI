using Azure.Data.Tables;
using MediatR;
using MudakAI.Chat.WebService.Repositories;
using MudakAI.Chat.WebService.Resources;
using MudakAI.Connectors.Azure.Table;
using MudakAI.Connectors.Discord;
using MudakAI.Connectors.OpenAI;
using MudakAI.TextToSpeech.Functions.Services;
using System.Globalization;
using System.Reflection;

namespace MudakAI.Chat.WebService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var settings = Settings.CreateFrom(builder.Configuration);

            // Configure localization
            ConfigureLocalization(settings.Localization);

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddDaprClient();
            builder.Services.AddTableStorage(settings.TableStorageConnectionString);

            builder.Services.AddDiscord(settings.Discord, interactionsAssembly: Assembly.GetExecutingAssembly());

            builder.Services.AddOpenAIChat(settings.OpenAI);

            builder.Services.AddTransient<BotConfigurationRepository>();

            builder.Services.AddTransient(
                sp => new ChatHistoryRepository(
                    sp.GetRequiredService<TableServiceClient>(), 
                    settings.MaxHistoryDepth, 
                    settings.MaxHistoryAge));

            builder.Services.AddTransient<TextToSpeechApiService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.Run();
        }

        private static void ConfigureLocalization(LocalizationSettings localizationSettings)
        {
            // Set the default culture from configuration
            if (!string.IsNullOrWhiteSpace(localizationSettings?.DefaultCulture))
            {
                var defaultCulture = new CultureInfo(localizationSettings.DefaultCulture);
                
                // Set the culture for the current thread
                CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
                CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;
                
                // Set the culture for resource lookups
                // Access the Culture property using reflection since it's internal
                var stringsType = typeof(Strings);
                var cultureProperty = stringsType.GetProperty("Culture", 
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                cultureProperty?.SetValue(null, defaultCulture);
            }
        }
    }
}