using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace MudakAI.Voice.Functions
{
    public class Settings
    {
        private readonly IConfiguration _configuration;

        private Settings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ServiceBusConnectionString => _configuration.GetValue<string>("ServiceBus");

        public string BlobStorageConnectionString => _configuration.GetValue<string>("BlobStorage");
        public string BlobStorageAudioContainerName => _configuration.GetValue<string>("BlobStorageAudioContainerName");
        public string BlobStorageLocksContainerName => _configuration.GetValue<string>("BlobStorageLocksContainerName");

        public string DiscordBotToken => _configuration.GetValue<string>("DiscordBotToken");

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
            RuleFor(x => x.ServiceBusConnectionString).NotEmpty();

            RuleFor(x => x.BlobStorageConnectionString).NotEmpty();
            RuleFor(x => x.BlobStorageAudioContainerName).NotEmpty();
            RuleFor(x => x.BlobStorageLocksContainerName).NotEmpty();

            RuleFor(x => x.DiscordBotToken).NotEmpty();
        }
    }
}
