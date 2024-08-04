using Discord.Interactions;
using MediatR;
using MudakAI.Chat.WebService.Repositories;

namespace MudakAI.Chat.WebService.CQRS.Commands
{
    public class SetPersonaCommand : IRequest<string>
    {
        public string GuildId { get; set; }

        public string Description { get; set; }

        public string Voice { get; set; }
    }

    public class SetPersonaHandler : IRequestHandler<SetPersonaCommand, string>
    {
        private readonly BotConfigurationRepository _botConfigurationRepository;

        public SetPersonaHandler(BotConfigurationRepository botConfigurationRepository)
        {
            _botConfigurationRepository = botConfigurationRepository;
        }

        public async Task<string> Handle(SetPersonaCommand command, CancellationToken cancellationToken)
        {
            await _botConfigurationRepository.SetPersona(command.GuildId, (command.Description, command.Voice));

            return $"Успішно задано опис персони: \"{command.Description}\", з голосом: \"{command.Voice}\"";
        }
    }

    public class SetPersonaDiscordModule : InteractionModuleBase
    {
        public enum VoiceSelection
        {
            Tetiana,
            Mykyta,
            Lada,
            Dmytro,
            Oleksa
        }

        private readonly IMediator _mediator;

        public SetPersonaDiscordModule(IMediator mediator)
        {
            _mediator = mediator;
        }

        [SlashCommand("set-persona", "Задати персону боту")]
        public async Task SetPersona(string description, VoiceSelection voice)
        {
            var response =
                await _mediator.Send(
                    new SetPersonaCommand 
                    { 
                        GuildId = Context.Guild.Id.ToString(), 
                        Description = description,
                        Voice = voice.ToString()
                    });

            await RespondAsync(response, ephemeral: true);
        }
    }
}
