using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MudakAI.Connectors.OpenAI.Services;
using OpenAI.Audio;

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
            services.AddSingleton(new AudioClient(settings.TTSModelId, settings.OpenAIApiKey));

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