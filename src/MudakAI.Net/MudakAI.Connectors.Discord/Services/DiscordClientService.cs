using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace MudakAI.Connectors.Discord.Services
{
    public class DiscordClientService
    {
        private readonly Assembly _interactionsAssembly;
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<DiscordClientService> _logger;
        private readonly Settings _settings;

        private readonly InteractionService _interactionService;

        public DiscordClientService(
            Assembly interactionsAssembly,
            IServiceProvider serviceProvider,
            ILogger<DiscordClientService> logger, 
            Settings settings,
            DiscordSocketClient discordSocketClient,
            InteractionService interactionService)
        {
            _interactionsAssembly = interactionsAssembly;
            _serviceProvider = serviceProvider;

            _logger = logger;
            _settings = settings;

            _interactionService = interactionService;
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

            if (_interactionsAssembly != null)
            {
                await _interactionService.AddModulesAsync(_interactionsAssembly, _serviceProvider);

                DiscordClient.GuildAvailable += async (guild) =>
                {
                    await _interactionService.RegisterCommandsToGuildAsync(guild.Id);

                    _logger.LogInformation("Discord socket client - Guild available: {guild}", guild.Name);
                };

                DiscordClient.InteractionCreated += async interaction =>
                {
                    var ctx = new SocketInteractionContext(DiscordClient, interaction);
                    await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
                };
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
