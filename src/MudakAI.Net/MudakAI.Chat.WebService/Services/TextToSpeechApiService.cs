using Dapr.Client;

namespace MudakAI.TextToSpeech.Functions.Services
{
    public class TextToSpeechApiService
    {
        private readonly DaprClient _daprClient;

        public TextToSpeechApiService(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }

        public async Task InitiateTextToSpeech(string uniqueId, string text)
        {
            var request = _daprClient.CreateInvokeMethodRequest(
                HttpMethod.Post,
                "mudakaitexttospeech-api",
                "/text-to-speech",
                new
                {
                    uniqueId,
                    text
                });

            var response = await _daprClient.InvokeMethodWithResponseAsync(request);

            response.EnsureSuccessStatusCode();
        }
    }
}
