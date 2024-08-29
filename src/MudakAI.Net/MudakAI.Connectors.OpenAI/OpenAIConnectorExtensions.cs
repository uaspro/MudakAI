using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MudakAI.Connectors.OpenAI.Services;

namespace MudakAI.Connectors.OpenAI
{
    public static class OpenAIConnectorExtensions
    {
        public static IServiceCollection AddOpenAIChat(this IServiceCollection services, Settings settings)
        {
            services.AddSingleton(new OpenAIChatCompletionService(settings.ModelId, settings.OpenAIApiKey));

            services.AddTransient<OpenAIChatService>();

            return services.AddOpenAIBase(settings);
        }

        public static IServiceCollection AddOpenAITTS(this IServiceCollection services, Settings settings)
        {
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            services.AddSingleton(new OpenAITextToAudioService(settings.TTSModelId, settings.OpenAIApiKey));
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            services.AddTransient<OpenAITextToSpeechService>();

            return services.AddOpenAIBase(settings);
        }
        private static IServiceCollection AddOpenAIBase(this IServiceCollection services, Settings settings)
        {
            services.AddSingleton(settings);

            return services;
        }
    }
}