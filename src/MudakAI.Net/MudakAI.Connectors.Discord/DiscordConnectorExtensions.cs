using Discord;
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
        public static IServiceCollection AddDiscord(this IServiceCollection services, Settings settings, bool registerEventListener = true, Assembly interactionsAssembly =  null)
        {
            var discordConfig = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.Guilds | GatewayIntents.GuildVoiceStates,
                EnableVoiceDaveEncryption = true
            };

            services.AddSingleton(discordConfig)
                    .AddSingleton(new DiscordSocketClient(discordConfig));

            services.AddSingleton(
                sp => new DiscordClientService(
                    interactionsAssembly,
                    sp.GetRequiredService<IServiceProvider>(),
                    sp.GetRequiredService<ILogger<DiscordClientService>>(),
                    settings,
                    sp.GetRequiredService<DiscordSocketClient>(),
                    sp.GetService<InteractionService>()));

            if (interactionsAssembly != null)
            {
                services.AddSingleton(
                    new InteractionServiceConfig
                    {
                        DefaultRunMode = RunMode.Async,
                        UseCompiledLambda = true,
                        AutoServiceScopes = true
                    });

                services.AddSingleton(sp => new InteractionService(
                    sp.GetRequiredService<DiscordSocketClient>(),
                    sp.GetRequiredService<InteractionServiceConfig>()));
            }

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