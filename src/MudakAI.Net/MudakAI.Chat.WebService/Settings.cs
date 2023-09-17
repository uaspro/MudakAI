using FluentValidation;

namespace MudakAI.Chat.WebService
{
    public class Settings
    {
        private readonly IConfiguration _configuration;

        private Settings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string TableStorageConnectionString => _configuration.GetConnectionString("TableStorage");

        public Connectors.Discord.Settings Discord => _configuration.GetSection(nameof(Discord)).Get<Connectors.Discord.Settings>();

        public Connectors.OpenAI.Settings OpenAI => _configuration.GetSection(nameof(OpenAI)).Get<Connectors.OpenAI.Settings>();

        public int MaxHistoryDepth => _configuration.GetValue<int>("MaxHistoryDepth");

        public TimeSpan MaxHistoryAge => _configuration.GetValue<TimeSpan>("MaxHistoryAge");

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
            RuleFor(x => x.TableStorageConnectionString).NotEmpty();

            RuleFor(x => x.Discord).NotNull();
            RuleFor(x => x.Discord.DiscordBotToken).NotEmpty();

            RuleFor(x => x.OpenAI).NotNull();
            RuleFor(x => x.OpenAI.OpenAIApiKey).NotEmpty();
            RuleFor(x => x.OpenAI.ModelId).NotEmpty();
            RuleFor(x => x.OpenAI.ResponseTokensLimit).GreaterThan(0);
        }
    }
}
