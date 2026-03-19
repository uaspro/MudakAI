using Dapr.Client;

namespace MudakAI.TextToSpeech.Functions.Services
{
    public record TextToSpeechRequest(string UniqueId, string Text, string Voice);

    public class TextToSpeechApiService
    {
        private readonly DaprClient _daprClient;

        public TextToSpeechApiService(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }

        public async Task InitiateTextToSpeech(string uniqueId, string text, string voice)
        {
            var request = _daprClient.CreateInvokeMethodRequest(
                "mudakaitexttospeech-api",
                "/text-to-speech",
                data: new TextToSpeechRequest(uniqueId, text, voice));

            var response = await _daprClient.InvokeMethodWithResponseAsync(request);

            response.EnsureSuccessStatusCode();
        }
    }
}
