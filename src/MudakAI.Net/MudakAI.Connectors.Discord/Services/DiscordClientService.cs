using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MudakAI.Connectors.Discord.Services
{
    public class DiscordClientService
    {
        private readonly ILogger<DiscordClientService> _logger;
        private readonly Settings _settings;

        public DiscordClientService(ILogger<DiscordClientService> logger, Settings settings, DiscordSocketClient discordSocketClient)
        {
            _logger = logger;
            _settings = settings;

            DiscordClient = discordSocketClient;
        }

        public DiscordSocketClient DiscordClient { get; }

        public bool IsReady { get; private set; }

        public async Task Connect()
        {
            if(DiscordClient.ConnectionState == ConnectionState.Connected)
            {
                return;
            }

            DiscordClient.Ready += async () =>
            {
                IsReady = true;

                _logger.LogInformation("Discord socket client - Ready!");
            };

            DiscordClient.Disconnected += async (ex) =>
            {
                _logger.LogInformation("Discord socket client - Disconnected. Failure: {failure}", ex);
            };

            _logger.LogInformation("Connecting to Discord...");

            await DiscordClient.LoginAsync(TokenType.Bot, _settings.DiscordBotToken);
            await DiscordClient.StartAsync();
        }
    }
}
