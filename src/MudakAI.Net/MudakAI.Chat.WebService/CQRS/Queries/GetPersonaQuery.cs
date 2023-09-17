using Discord.Interactions;
using MediatR;
using MudakAI.Chat.WebService.Repositories;

namespace MudakAI.Chat.WebService.CQRS.Queries
{
    public class GetPersonaQuery : IRequest<string>
    {
        public string GuildId { get; set; }
    }

    public class GetPersonaHandler : IRequestHandler<GetPersonaQuery, string>
    {
        private readonly BotConfigurationRepository _botConfigurationRepository;

        public GetPersonaHandler(BotConfigurationRepository botConfigurationRepository)
        {
            _botConfigurationRepository = botConfigurationRepository;
        }

        public async Task<string> Handle(GetPersonaQuery request, CancellationToken cancellationToken)
        {
            var response = await _botConfigurationRepository.GetPersona(request.GuildId);

            return $"Поточний опис персони: \"{response.Description}\", з голосом: \"{response.Voice}\"";
        }
    }

    public class GetPersonaDiscordModule : InteractionModuleBase
    {
        private readonly IMediator _mediator;

        public GetPersonaDiscordModule(IMediator mediator)
        {
            _mediator = mediator;
        }

        [SlashCommand("get-persona-test", "Отримати опис поточної персони бота")]
        public async Task GetPersona()
        {
            var response = 
                await _mediator.Send(
                    new GetPersonaQuery 
                    {
                        GuildId = Context.Guild.Id.ToString() 
                    });

            await RespondAsync(response, ephemeral: true);
        }
    }
}
