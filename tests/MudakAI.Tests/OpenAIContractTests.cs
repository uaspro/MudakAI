using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MudakAI.Connectors.OpenAI;
using MudakAI.Connectors.OpenAI.Services;
using MudakAI.TextToSpeech.Functions.Services;
using OpenAI;
using OpenAI.Audio;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MudakAI.Tests
{
    public class OpenAIContractTests
    {
        [Fact]
        public async Task ChatKeepsTheConfiguredModelHistoryAndResponseTokenLimit()
        {
            var settings = new Settings { ModelId = "gpt-4.1-mini", ResponseTokensLimit = 500 };
            using var handler = new ContractHandler(async request =>
            {
                Assert.Equal("/v1/chat/completions", request.RequestUri.AbsolutePath);
                using var json = JsonDocument.Parse(await request.Content.ReadAsStringAsync());
                Assert.Equal(settings.ModelId, json.RootElement.GetProperty("model").GetString());
                Assert.Equal(500, json.RootElement.GetProperty("max_completion_tokens").GetInt32());
                var messages = json.RootElement.GetProperty("messages");
                Assert.Equal("system", messages[0].GetProperty("role").GetString());
                Assert.Equal("Keep the persona", messages[0].GetProperty("content").GetString());
                Assert.Equal("Привіт", messages[1].GetProperty("content").GetString());
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""
                        {"id":"chat-test","object":"chat.completion","created":0,"model":"gpt-4.1-mini","choices":[{"index":0,"message":{"role":"assistant","content":"Вітаю!"},"finish_reason":"stop"}]}
                        """, Encoding.UTF8, "application/json")
                };
            });
            using var client = new HttpClient(handler);
            var connector = new OpenAIChatCompletionService(settings.ModelId, "test-key", httpClient: client);
            var service = new OpenAIChatService(NullLogger<OpenAIChatService>.Instance, settings, connector);
            var history = new ChatHistory("Keep the persona");
            history.AddUserMessage("Привіт");

            var response = await service.GenerateResponse(history);

            Assert.Equal(AuthorRole.Assistant, response.Role);
            Assert.Equal("Вітаю!", response.Content);
        }

        [Theory]
        [InlineData("Male", "ash")]
        [InlineData("male", "ash")]
        [InlineData("Female", "coral")]
        public async Task SpeechKeepsVoiceSelectionAndWavFormatForDiscordPlayback(string voice, string expectedVoice)
        {
            var settings = new Settings { TTSModelId = "gpt-4o-mini-tts" };
            var audio = Encoding.ASCII.GetBytes("RIFF-test-WAVE");
            using var handler = new ContractHandler(async request =>
            {
                Assert.Equal("/v1/audio/speech", request.RequestUri.AbsolutePath);
                using var json = JsonDocument.Parse(await request.Content.ReadAsStringAsync());
                Assert.Equal(settings.TTSModelId, json.RootElement.GetProperty("model").GetString());
                Assert.Equal("Привіт", json.RootElement.GetProperty("input").GetString());
                Assert.Equal(expectedVoice, json.RootElement.GetProperty("voice").GetString());
                Assert.Equal("wav", json.RootElement.GetProperty("response_format").GetString());
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(audio) };
            });
            using var client = new HttpClient(handler);
            var connector = new AudioClient(settings.TTSModelId, new ApiKeyCredential("test-key"),
                new OpenAIClientOptions { Transport = new HttpClientPipelineTransport(client) });
            var speech = new OpenAITextToSpeechService(NullLogger<OpenAIChatService>.Instance, settings, connector);
            var service = new SpeechGenerationApiService(null, speech);

            using var result = await service.GenerateSpeech("Привіт", voice, TextToSpeechEngine.OpenAI);
            using var buffer = new MemoryStream();
            await result.CopyToAsync(buffer, TestContext.Current.CancellationToken);

            Assert.Equal(audio, buffer.ToArray());
        }

        private sealed class ContractHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _send;

            public ContractHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send)
            {
                _send = send;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _send(request);
            }
        }
    }
}
