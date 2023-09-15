using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Azure;
using MudakAI.Connectors.Azure.Blob;
using MudakAI.TextToSpeech.Functions.Services;
using System.Text.Json;
using System.Threading.Tasks;

namespace MudakAI.TextToSpeech.Functions
{
    public class TextToSpeechRequest
    {
        public string UniqueId { get; set; }

        public string Text { get; set; }
    }

    public class PlaybackRequest
    {
        public string AudioBlobName { get; set; }
    }

    public class TextToSpeechProcessingFunction
    {
        private readonly SpeechGenerationApiService _speechGenerationApiService;
        private readonly BlobStorageService _blobStorageService;

        private readonly ServiceBusSender _serviceBusPlaybackQueueSender;

        public TextToSpeechProcessingFunction(
            Settings settings,
            SpeechGenerationApiService textToSpeechService, 
            BlobStorageService blobStorageService,
            IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory)
        {
            _speechGenerationApiService = textToSpeechService;
            _blobStorageService = blobStorageService;

            _serviceBusPlaybackQueueSender = serviceBusSenderFactory.CreateClient(settings.ServiceBusPlaybackQueueName);
        }

        [FunctionName("TextToSpeechProcessingFunction")]
        public async Task Run([ServiceBusTrigger("text-to-speech", Connection = "ServiceBusListen")] TextToSpeechRequest textToSpeechRequest)
        {
            var audioStream = await _speechGenerationApiService.GenerateSpeech(textToSpeechRequest.Text);

            await _blobStorageService.Upload(textToSpeechRequest.UniqueId, audioStream);

            await _serviceBusPlaybackQueueSender.SendMessageAsync(
                new ServiceBusMessage(JsonSerializer.Serialize(
                    new PlaybackRequest
                    {
                        AudioBlobName = textToSpeechRequest.UniqueId
                    })));
        }
    }
}
