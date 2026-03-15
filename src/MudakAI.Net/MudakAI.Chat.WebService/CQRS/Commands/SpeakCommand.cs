using Discord;
using Discord.Interactions;
using MudakAI.Chat.WebService.Resources;
using MudakAI.TextToSpeech.Functions.Services;
using System.Text.RegularExpressions;

namespace MudakAI.Chat.WebService.CQRS.Commands
{
    public class SpeakDiscordModule : InteractionModuleBase
    {
        private const int MinTextLength = 2;
        private const int MaxTextLength = 500;
        private const string CommandName = "speak";
        private const string CommandDescription = "Озвучити текст";

        private static readonly string MinTextValidationPattern = @$"([a-zA-Zа-яА-Я]){{{MinTextLength},}}";

        public enum VoiceSelection
        {
            Male,
            Female
        }

        private readonly TextToSpeechApiService _textToSpeechApiService;

        public SpeakDiscordModule(TextToSpeechApiService textToSpeechApiService)
        {
            _textToSpeechApiService = textToSpeechApiService;
        }

        [SlashCommand(CommandName, CommandDescription)]
        public async Task SetPersona(string text, VoiceSelection voice)
        {
            var voiceChannel = Context.Channel as IVoiceChannel;
            if (voiceChannel == null)
            {
                await RespondAsync(Strings.Speak_NotVoiceChannel, ephemeral: true);

                return;
            }

            if (text.Length < MinTextLength || !Regex.IsMatch(text, MinTextValidationPattern))
            {
                await RespondAsync(string.Format(Strings.Speak_MinTextLength, MinTextLength), ephemeral: true);

                return;
            }

            if (text.Length > MaxTextLength)
            {
                await RespondAsync(string.Format(Strings.Speak_MaxTextLength, MaxTextLength), ephemeral: true);

                return;
            }

            await _textToSpeechApiService.InitiateTextToSpeech(
                $"{Context.Guild.Id}_{Context.Channel.Id}_{Guid.NewGuid().ToString("N")}",
                text,
                voice.ToString());

            await RespondAsync(Strings.Speak_Success, ephemeral: true);
        }
    }
}
