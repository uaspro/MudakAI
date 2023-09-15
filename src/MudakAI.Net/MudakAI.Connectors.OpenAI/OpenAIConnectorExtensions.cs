using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using MudakAI.Connectors.OpenAI;
using MudakAI.Connectors.OpenAI.Services;

namespace MudakAI.Connectors.Discord
{
    public static class OpenAIConnectorExtensions
    {
        public static IServiceCollection AddOpenAI(this IServiceCollection services, Settings settings)
        {
            services.AddSingleton(new OpenAIChatCompletion(settings.ModelId, settings.OpenAIApiKey));

            services.AddSingleton(settings);
            services.AddTransient<OpenAIChatService>();

            return services;
        }
    }
}