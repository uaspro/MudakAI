using Azure.Data.Tables;
using MediatR;
using MudakAI.Chat.WebService.CQRS.Commands;
using MudakAI.Chat.WebService.Repositories;
using MudakAI.Connectors.Azure.Table;
using MudakAI.Connectors.Discord;
using MudakAI.TextToSpeech.Functions.Services;
using System.Reflection;

namespace MudakAI.Chat.WebService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var settings = Settings.CreateFrom(builder.Configuration);

            builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddDaprClient();
            builder.Services.AddTableStorage(settings.TableStorageConnectionString);

            builder.Services.AddDiscord(Assembly.GetExecutingAssembly(), settings.Discord);

            builder.Services.AddOpenAI(settings.OpenAI);

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
    }
}