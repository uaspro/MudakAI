using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using MudakAI.TextToSpeech.API.DTOs;
using System.Text.Json;

namespace MudakAI.TextToSpeech.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var settings = Settings.CreateFrom(builder.Configuration);

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddServiceBusClient(settings.ServiceBusConnectionString);

                clientBuilder.AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) =>
                        provider.GetService<ServiceBusClient>()
                                .CreateSender(settings.TextToSpeechQueueName))
                    .WithName(settings.TextToSpeechQueueName);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapPost("/text-to-speech", async (TextToSpeechRequest request, IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory) =>
            {
                var textToSpeechQueueSender = serviceBusSenderFactory.CreateClient(settings.TextToSpeechQueueName);

                await textToSpeechQueueSender.SendMessageAsync(
                    new ServiceBusMessage(JsonSerializer.Serialize(request)));

                return Results.Accepted();
            })
            .WithName("InitTextToSpeech");

            app.Run();
        }
    }
}