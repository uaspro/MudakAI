using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MudakAI.TextToSpeech.Functions.Services;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MudakAI.Tests
{
    public class DaprContractTests
    {
        [Theory]
        [InlineData("Male", "dmytro")]
        [InlineData("Female", "tetiana")]
        public async Task DefaultSpeechStillPostsJsonToThePythonDaprApplication(string voice, string expectedVoice)
        {
            var builder = WebApplication.CreateSlimBuilder();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            await using var app = builder.Build();
            string receivedBody = null;
            var audio = Encoding.ASCII.GetBytes("RIFF-test-WAVE");
            app.MapPost("/v1.0/invoke/tts-api/method/tts/", async (HttpRequest request) =>
            {
                receivedBody = await new StreamReader(request.Body).ReadToEndAsync();
                return Results.Bytes(audio, "audio/wav");
            });
            await app.StartAsync(TestContext.Current.CancellationToken);
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>().Addresses.Single();
            using var dapr = new DaprClientBuilder().UseHttpEndpoint(address).Build();
            var service = new SpeechGenerationApiService(dapr, null);

            using var result = await service.GenerateSpeech("Привіт", voice);
            using var buffer = new MemoryStream();
            await result.CopyToAsync(buffer, TestContext.Current.CancellationToken);

            Assert.Equal(audio, buffer.ToArray());
            using var json = JsonDocument.Parse(receivedBody);
            Assert.Equal("Привіт", json.RootElement.GetProperty("text").GetString());
            Assert.Equal(expectedVoice, json.RootElement.GetProperty("voice").GetString());
        }
    }
}
