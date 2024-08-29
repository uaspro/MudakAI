namespace MudakAI.Connectors.OpenAI
{
    public class Settings
    {
        public string OpenAIApiKey { get; set; }

        public string ModelId { get; set; }

        public string TTSModelId { get; set; }

        public int ResponseTokensLimit { get; set; }
    }
}
