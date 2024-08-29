using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToAudio;
using System.Collections.Generic;
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
        Shimmer
    }

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public class OpenAITextToSpeechService
    {
        private readonly ILogger<OpenAIChatService> _logger;

        private readonly Settings _settings;

        private readonly OpenAITextToAudioService _openAITTS;

        public OpenAITextToSpeechService(ILogger<OpenAIChatService> logger, Settings settings, OpenAITextToAudioService openAITTS)
        {
            _logger = logger;

            _settings = settings;
            _openAITTS = openAITTS;
        }

        public async Task<Stream> GenerateSpeech(string text, OpenAITextToSpeechVoice voice = OpenAITextToSpeechVoice.Onyx)
        {
            var response = 
                await _openAITTS.GetAudioContentAsync(
                    text, 
                    new PromptExecutionSettings
                    {
                        ExtensionData = new Dictionary<string, object>
                        {
                            { "voice", voice.ToString().ToLowerInvariant() },
                            { "response_format", "wav" }
                        }
                    });

            _logger.LogInformation("OpenAI tts generated for: {text}", text);

            return new MemoryStream(response.Data.Value.ToArray());
        }
    }
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}
