using Microsoft.Azure.WebJobs;
using MudakAI.Connectors.Discord.Services;
using System.Threading.Tasks;

namespace MudakAI.Voice.Functions
{
    public class DiscordConnectorFunction
    {
        private readonly DiscordClientService _discordClientService;

        public DiscordConnectorFunction(DiscordClientService discordClientService)
        {
            _discordClientService = discordClientService;
        }

        [FunctionName("DiscordConnectorFunction")]
        public Task Run([TimerTrigger("0 * * * *", RunOnStartup = true)]TimerInfo timer)
        {
            return _discordClientService.Connect();
        }
    }
}
