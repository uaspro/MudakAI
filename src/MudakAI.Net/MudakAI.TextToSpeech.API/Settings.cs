using FluentValidation;

namespace MudakAI.TextToSpeech.API
{
    public class Settings
    {
        private readonly IConfiguration _configuration;

        private Settings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ServiceBusConnectionString => _configuration.GetConnectionString("ServiceBus");

        public string TextToSpeechQueueName => _configuration.GetValue<string>("TextToSpeechQueueName");

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
            RuleFor(x => x.TextToSpeechQueueName).NotEmpty();
        }
    }
}
