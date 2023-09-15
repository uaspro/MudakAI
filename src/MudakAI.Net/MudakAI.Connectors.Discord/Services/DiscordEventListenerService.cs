using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MudakAI.Connectors.Discord.CQRS.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace MudakAI.Connectors.Discord.Services
{
    public class DiscordEventListenerService : BackgroundService
    {
        private readonly ILogger<DiscordEventListenerService> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        private readonly DiscordClientService _discordClientService;

        public DiscordEventListenerService(
            ILogger<DiscordEventListenerService> logger,
            IServiceScopeFactory serviceScope,
            DiscordClientService discordClientService)
        {
            _logger = logger;
            _serviceScope = serviceScope;

            _discordClientService = discordClientService;
        }

        private IMediator Mediator
        {
            get
            {
                var scope = _serviceScope.CreateScope();
                return scope.ServiceProvider.GetRequiredService<IMediator>();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _discordClientService.DiscordClient.MessageReceived += 
                (message) => Mediator.Publish(new MessageReceivedNotification(message));

            await _discordClientService.Connect();

            _logger.LogInformation("Discord event listener started");

            // Block this task until the service is running
            await Task.Delay(-1);
        }
    }
}
