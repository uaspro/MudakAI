using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
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

        public string Voice { get; set; }
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

        [Function("TextToSpeechProcessingFunction")]
        public async Task Run([ServiceBusTrigger("text-to-speech", Connection = "ServiceBusListen")] TextToSpeechRequest textToSpeechRequest)
        {
            var textToSpeechEngine = TextToSpeechEngine.Default;
            var voice = textToSpeechRequest.Voice;

            if ( textToSpeechEngine == TextToSpeechEngine.OpenAI)
            {
               voice = string.Equals(voice, "Male", System.StringComparison.InvariantCultureIgnoreCase) ? "cedar" : "marin";
            }

            var audioStream = 
                await _speechGenerationApiService.GenerateSpeech(textToSpeechRequest.Text, voice, textToSpeechEngine);

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
