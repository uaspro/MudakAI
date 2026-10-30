using Azure.Messaging.ServiceBus;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MudakAI.Tests
{
    public class TextToSpeechApiTests
    {
        [Fact]
        public void GeneratedAudioMessagesRetainTheBlobNameExpectedByTheVoiceWorker()
        {
            var json = JsonSerializer.Serialize(new TextToSpeech.Functions.PlaybackRequest
            {
                AudioBlobName = "guild_channel_message"
            });

            var request = JsonSerializer.Deserialize<Voice.Functions.PlaybackRequest>(json);

            Assert.Equal("guild_channel_message", request.AudioBlobName);
        }

        [Fact]
        public async Task AcceptedSpeechRetainsTheQueueContractConsumedByTheWorker()
        {
            ServiceBusMessage queuedMessage = null;
            var sender = new Mock<ServiceBusSender>();
            sender.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
                .Callback<ServiceBusMessage, CancellationToken>((message, _) => queuedMessage = message)
                .Returns(Task.CompletedTask);

            using var factory = CreateFactory(sender.Object);
            using var client = factory.CreateClient();
            using var response = await client.PostAsJsonAsync("/text-to-speech", new
            {
                UniqueId = "guild_channel_message",
                Text = "Привіт!",
                Voice = "Male"
            }, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            Assert.NotNull(queuedMessage);
            var json = queuedMessage.Body.ToString();
            Assert.Contains("\"UniqueId\"", json);
            var request = JsonSerializer.Deserialize<TextToSpeech.Functions.TextToSpeechRequest>(json);
            Assert.Equal("guild_channel_message", request.UniqueId);
            Assert.Equal("Привіт!", request.Text);
            Assert.Equal("Male", request.Voice);
            sender.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task MalformedRequestsDoNotReachTheSpeechQueue()
        {
            var sender = new Mock<ServiceBusSender>(MockBehavior.Strict);
            using var factory = CreateFactory(sender.Object);
            using var client = factory.CreateClient();
            using var body = new StringContent("{", Encoding.UTF8, "application/json");
            using var response = await client.PostAsync("/text-to-speech", body, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            sender.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task SwaggerStillDescribesTheSpeechEndpoint()
        {
            using var factory = CreateFactory(Mock.Of<ServiceBusSender>());
            using var client = factory.CreateClient();
            using var document = JsonDocument.Parse(await client.GetStringAsync("/swagger/v1/swagger.json", TestContext.Current.CancellationToken));
            var operation = document.RootElement.GetProperty("paths").GetProperty("/text-to-speech").GetProperty("post");

            Assert.Equal("InitTextToSpeech", operation.GetProperty("operationId").GetString());
            Assert.True(operation.GetProperty("requestBody").GetProperty("content").TryGetProperty("application/json", out _));
        }

        [Theory]
        [InlineData("ConnectionStrings:ServiceBus")]
        [InlineData("TextToSpeechQueueName")]
        public void MissingQueueConfigurationStillFailsAtStartup(string missingKey)
        {
            var values = new Dictionary<string, string>
            {
                ["ConnectionStrings:ServiceBus"] = "Endpoint=sb://example.test/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==",
                ["TextToSpeechQueueName"] = "text-to-speech"
            };
            values.Remove(missingKey);
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

            Assert.Throws<ValidationException>(() => TextToSpeech.API.Settings.CreateFrom(configuration));
        }

        private static WebApplicationFactory<TextToSpeech.API.Program> CreateFactory(ServiceBusSender sender)
        {
            return new WebApplicationFactory<TextToSpeech.API.Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseSetting("ConnectionStrings:ServiceBus", "Endpoint=sb://example.test/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==");
                builder.UseSetting("TextToSpeechQueueName", "text-to-speech");
                builder.ConfigureTestServices(services =>
                {
                    var senderFactory = new Mock<IAzureClientFactory<ServiceBusSender>>();
                    senderFactory.Setup(f => f.CreateClient("text-to-speech")).Returns(sender);
                    services.AddSingleton(senderFactory.Object);
                });
            });
        }
    }
}
