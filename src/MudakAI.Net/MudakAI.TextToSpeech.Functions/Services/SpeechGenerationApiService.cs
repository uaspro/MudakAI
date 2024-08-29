using Dapr.Client;
using MudakAI.Connectors.OpenAI.Services;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MudakAI.TextToSpeech.Functions.Services
{
    public enum TextToSpeechEngine
    {
        Default = 0,
        OpenAI
    }

    public enum VoiceSelection
    {
        Male,
        Female
    }

    public enum DefaultTextToSpeechVoice
    {
        None,
        Tetiana,
        Mykyta,
        Lada,
        Dmytro
    }

    public class SpeechGenerationApiService
    {
        private readonly DaprClient _daprClient;
        private readonly OpenAITextToSpeechService _openAITextToSpeechService;

        public SpeechGenerationApiService(DaprClient daprClient, OpenAITextToSpeechService openAITextToSpeechService)
        {
            _daprClient = daprClient;
            _openAITextToSpeechService = openAITextToSpeechService;
        }

        public Task<Stream> GenerateSpeech(string text, string voice, TextToSpeechEngine textToSpeechEngine = TextToSpeechEngine.Default)
        {
            switch (textToSpeechEngine)
            {
                case TextToSpeechEngine.OpenAI:
                    var openAIVoice =
                        string.Equals(voice, VoiceSelection.Male.ToString(), System.StringComparison.InvariantCultureIgnoreCase)
                            ? OpenAITextToSpeechVoice.Onyx
                            : OpenAITextToSpeechVoice.Nova;

                    return GenerateSpeechOpenAI(text, openAIVoice);
                default:
                    var defaultVoice =
                        string.Equals(voice, VoiceSelection.Male.ToString(), System.StringComparison.InvariantCultureIgnoreCase)
                            ? DefaultTextToSpeechVoice.Dmytro
                            : DefaultTextToSpeechVoice.Tetiana;

                    return GenerateSpeechDefault(text, defaultVoice);
            }
        }

        private async Task<Stream> GenerateSpeechOpenAI(string text, OpenAITextToSpeechVoice voice)
        {
            return await _openAITextToSpeechService.GenerateSpeech(text, voice);
        }

        private async Task<Stream> GenerateSpeechDefault(string text, DefaultTextToSpeechVoice voice)
        {
            var request = _daprClient.CreateInvokeMethodRequest(
                HttpMethod.Post,
                "tts-api",
                "/tts/",
                new
                {
                    text,
                    voice = voice.ToString().ToLowerInvariant()
                });

            var response = await _daprClient.InvokeMethodWithResponseAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
    }
}
