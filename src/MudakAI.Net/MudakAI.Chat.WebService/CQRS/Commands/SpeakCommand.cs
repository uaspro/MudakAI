using Discord;
using Discord.Interactions;
using MudakAI.TextToSpeech.Functions.Services;
using System.Text.RegularExpressions;

namespace MudakAI.Chat.WebService.CQRS.Commands
{
    public class SpeakDiscordModule : InteractionModuleBase
    {
        private const int MinTextLength = 2;
        private const int MaxTextLength = 500;

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

        [SlashCommand("speak", "Озвучити текст")]
        public async Task SetPersona(string text, VoiceSelection voice)
        {
            var voiceChannel = Context.Channel as IVoiceChannel;
            if (voiceChannel == null)
            {
                await RespondAsync("Текст можна озвучити лише в голосовому чаті.", ephemeral: true);

                return;
            }

            if (text.Length < MinTextLength || !Regex.IsMatch(text, MinTextValidationPattern))
            {
                await RespondAsync($"Текст повинен містити хоча б {MinTextLength} українські чи англійські літери.", ephemeral: true);

                return;
            }

            if (text.Length > MaxTextLength)
            {
                await RespondAsync($"Текст має бути коротшим за {MaxTextLength} символів.", ephemeral: true);

                return;
            }

            await _textToSpeechApiService.InitiateTextToSpeech(
                $"{Context.Guild.Id}_{Context.Channel.Id}_{Guid.NewGuid().ToString("N")}",
                text,
                voice.ToString());

            await RespondAsync("Прийнято, текст скоро буде озвучений!", ephemeral: true);
        }
    }
}
