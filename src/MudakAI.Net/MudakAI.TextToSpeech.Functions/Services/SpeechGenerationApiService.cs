using MudakAI.Connectors.OpenAI.Services;
using System.IO;
using System.Threading.Tasks;

namespace MudakAI.TextToSpeech.Functions.Services
{
    public enum VoiceSelection
    {
        Male,
        Female
    }

    public class SpeechGenerationApiService
    {
        private readonly OpenAITextToSpeechService _openAITextToSpeechService;

        public SpeechGenerationApiService(OpenAITextToSpeechService openAITextToSpeechService)
        {
            _openAITextToSpeechService = openAITextToSpeechService;
        }

        public Task<Stream> GenerateSpeech(string text, string voice)
        {
            var openAIVoice =
                string.Equals(voice, VoiceSelection.Male.ToString(), System.StringComparison.InvariantCultureIgnoreCase)
                    ? OpenAITextToSpeechVoice.Ash
                    : OpenAITextToSpeechVoice.Coral;

            return _openAITextToSpeechService.GenerateSpeech(text, openAIVoice);
        }
    }
}