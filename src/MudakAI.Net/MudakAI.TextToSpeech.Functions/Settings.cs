using FluentValidation;
using Microsoft.Extensions.Configuration;
using MudakAI.TextToSpeech.Functions.Services;

namespace MudakAI.TextToSpeech.Functions
{
    public class Settings
    {
        private readonly IConfiguration _configuration;

        private Settings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ServiceBusListenConnectionString => _configuration.GetValue<string>("ServiceBusListen");
        public string ServiceBusSendConnectionString => _configuration.GetValue<string>("ServiceBusSend");
        public string ServiceBusPlaybackQueueName => _configuration.GetValue<string>("ServiceBusPlaybackQueueName");

        public string BlobStorageConnectionString => _configuration.GetValue<string>("BlobStorage");
        public string BlobStorageAudioContainerName => _configuration.GetValue<string>("BlobStorageAudioContainerName");

        public SpeechGenerationApiService.Settings TextToSpeech => _configuration.GetSection(nameof(TextToSpeech)).Get<SpeechGenerationApiService.Settings>();

        public static Settings CreateFrom(IConfiguration configuration)
        {
            var settings = new Settings(configuration);

            var settingsValidator = new SettingsValidator();
            settingsValidator.ValidateAndThrow(settings);

            return settings;
        }
    }

    public class SettingsValidator : AbstractValidator<Settings>
    {
        public SettingsValidator()
        {
            RuleFor(x => x.ServiceBusListenConnectionString).NotEmpty();
            RuleFor(x => x.ServiceBusSendConnectionString).NotEmpty();
            RuleFor(x => x.ServiceBusPlaybackQueueName).NotEmpty();

            RuleFor(x => x.BlobStorageConnectionString).NotEmpty();
            RuleFor(x => x.BlobStorageAudioContainerName).NotEmpty();

            RuleFor(x => x.TextToSpeech).NotNull();
            RuleFor(x => x.TextToSpeech.Speed).GreaterThanOrEqualTo(0.5m);
            RuleFor(x => x.TextToSpeech.Speed).LessThanOrEqualTo(2m);
        }
    }
}
