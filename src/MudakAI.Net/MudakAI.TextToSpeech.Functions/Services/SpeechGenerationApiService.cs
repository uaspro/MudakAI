using Dapr.Client;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MudakAI.TextToSpeech.Functions.Services
{
    public enum TextToSpeechVoice
    {
        None,
        Tetiana,
        Mykyta,
        Lada,
        Dmytro
    }

    public class SpeechGenerationApiService
    {
        public class Settings
        {
            public TextToSpeechVoice Voice { get; set; }

            public decimal Speed { get; set; }
        }

        private readonly Settings _settings;
        private readonly DaprClient _daprClient;

        public SpeechGenerationApiService(Settings settings, DaprClient daprClient)
        {
            _settings = settings;
            _daprClient = daprClient;
        }

        public async Task<Stream> GenerateSpeech(string text)
        {
            var request = _daprClient.CreateInvokeMethodRequest(
                HttpMethod.Post,
                "tts-api",
                "/tts/",
                new
                {
                    text,
                    voice = _settings.Voice.ToString().ToLowerInvariant(),
                    speed = _settings.Speed
                });

            var response = await _daprClient.InvokeMethodWithResponseAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
    }
}
