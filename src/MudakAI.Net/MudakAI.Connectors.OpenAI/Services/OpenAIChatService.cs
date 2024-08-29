using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MudakAI.Connectors.OpenAI.Services
{
    public class OpenAIChatService
    {
        private readonly ILogger<OpenAIChatService> _logger;

        private readonly Settings _settings;
        private readonly OpenAIChatCompletionService _openAIChat;

        public OpenAIChatService(ILogger<OpenAIChatService> logger, Settings settings, OpenAIChatCompletionService openAIChat)
        {
            _logger = logger;

            _settings = settings;
            _openAIChat = openAIChat;
        }

        public async Task<ChatMessageContent> GenerateResponse(ChatHistory chatHistory)
        {
            var chatReply = 
                await _openAIChat.GetChatMessageContentAsync(
                    chatHistory,
                    new PromptExecutionSettings
                    {
                        ExtensionData = new Dictionary<string, object>() 
                        {
                            { "MaxTokens", _settings.ResponseTokensLimit }
                        }
                    });

            _logger.LogInformation("OpenAI chat response generated: {chatReply}", chatReply);

            return new ChatMessageContent(AuthorRole.Assistant, chatReply.Items);
        }
    }
}
