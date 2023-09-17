using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.AzureSdk;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MudakAI.Connectors.OpenAI.Services
{
    public class OpenAIChatService
    {
        private readonly ILogger<OpenAIChatService> _logger;

        private readonly Settings _settings;
        private readonly OpenAIChatCompletion _openAIChat;

        public OpenAIChatService(ILogger<OpenAIChatService> logger, Settings settings, OpenAIChatCompletion openAIChat)
        {
            _logger = logger;

            _settings = settings;
            _openAIChat = openAIChat;
        }

        public async Task<ChatMessage> GenerateResponse(IEnumerable<ChatMessage> chatHistory)
        {
            var chat = _openAIChat.CreateNewChat();

            chat.AddRange(chatHistory.Select(m => new SKChatMessage(m)));

            var chatReply = 
                await _openAIChat.GenerateMessageAsync(
                    chat,
                    new ChatRequestSettings
                    {
                        MaxTokens = _settings.ResponseTokensLimit
                    });

            _logger.LogInformation("OpenAI chat response generated: {chatReply}", chatReply);

            return new ChatMessage(ChatRole.Assistant, chatReply);
        }
    }
}
