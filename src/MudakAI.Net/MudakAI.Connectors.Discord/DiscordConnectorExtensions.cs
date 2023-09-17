using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudakAI.Connectors.Discord.Services;
using System;
using System.Reflection;

namespace MudakAI.Connectors.Discord
{
    public static class DiscordConnectorExtensions
    {
        public static IServiceCollection AddDiscord(this IServiceCollection services, Assembly interactionsAssembly, Settings settings, bool registerEventListener = true)
        {
            var discordConfig = new DiscordSocketConfig();
            services.AddSingleton(discordConfig)
                    .AddSingleton<DiscordSocketClient>();

            services.AddSingleton(
                new InteractionServiceConfig
                {
                    DefaultRunMode = RunMode.Async,
                    UseCompiledLambda = true,
                    AutoServiceScopes = true
                });

            services.AddSingleton<InteractionService>();

            services.AddSingleton(
                sp => new DiscordClientService(
                    interactionsAssembly,
                    sp.GetRequiredService<IServiceProvider>(),
                    sp.GetRequiredService<ILogger<DiscordClientService>>(),
                    settings,
                    sp.GetRequiredService<DiscordSocketClient>(),
                    sp.GetRequiredService<InteractionService>()));

            if (registerEventListener)
            {
                services.AddSingleton<DiscordEventListenerService>();

                services.AddHostedService(
                    provider => provider.GetRequiredService<DiscordEventListenerService>());
            }

            return services;
        }
    }
}