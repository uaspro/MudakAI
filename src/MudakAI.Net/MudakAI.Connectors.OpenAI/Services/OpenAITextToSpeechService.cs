using Microsoft.Extensions.Logging;
using OpenAI.Audio;
using System.IO;
using System.Threading.Tasks;

namespace MudakAI.Connectors.OpenAI.Services
{
    public enum OpenAITextToSpeechVoice
    {
        Alloy,
        Echo,
        Fable,
        Onyx,
        Nova,
        Shimmer,
        Ash,
        Coral
    }

    public class OpenAITextToSpeechService
    {
        private readonly ILogger<OpenAIChatService> _logger;

        private readonly Settings _settings;

        private readonly AudioClient _openAITTS;

        public OpenAITextToSpeechService(ILogger<OpenAIChatService> logger, Settings settings, AudioClient openAITTS)
        {
            _logger = logger;

            _settings = settings;
            _openAITTS = openAITTS;
        }

        public async Task<Stream> GenerateSpeech(string text, OpenAITextToSpeechVoice voice = OpenAITextToSpeechVoice.Onyx)
        {
            var response = 
                await _openAITTS.GenerateSpeechAsync(
                    text,
                    new GeneratedSpeechVoice(voice.ToString().ToLowerInvariant()),
                    new SpeechGenerationOptions
                    {
                        ResponseFormat = GeneratedSpeechFormat.Wav
                    });

            _logger.LogInformation("OpenAI tts generated for: {text}", text);

            return new MemoryStream(response.Value.ToArray());
        }
    }
}
