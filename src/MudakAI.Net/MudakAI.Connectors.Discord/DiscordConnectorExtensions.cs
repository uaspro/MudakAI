using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MudakAI.Connectors.Discord.Services;

namespace MudakAI.Connectors.Discord
{
    public static class DiscordConnectorExtensions
    {
        public static IServiceCollection AddDiscord(this IServiceCollection services, Settings settings, bool registerEventListener = true)
        {
            var discordConfig = new DiscordSocketConfig();
            services.AddSingleton(discordConfig)
                    .AddSingleton<DiscordSocketClient>();

            services.AddSingleton<DiscordClientService>();

            services.AddSingleton(settings);

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