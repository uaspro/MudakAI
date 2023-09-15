using Azure.AI.OpenAI;
using Discord;
using Discord.WebSocket;
using MudakAI.Chat.WebService.Services;
using MudakAI.Connectors.Discord.CQRS.Notifications;
using MudakAI.Connectors.OpenAI.Services;
using MudakAI.TextToSpeech.Functions.Services;

namespace MudakAI.Chat.WebService.CQRS.Notifications
{
    public class DiscordMessageReceivedNotificationHandler : MessageReceivedNotificationHandlerBase
    {
        private readonly ILogger<DiscordMessageReceivedNotificationHandler> _logger;

        private readonly ChatHistoryRepository _chatHistoryRepository;
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly OpenAIChatService _openAIChatService;
        private readonly TextToSpeechApiService _textToSpeechApiService;

        public DiscordMessageReceivedNotificationHandler(
            ILogger<DiscordMessageReceivedNotificationHandler> logger, 
            ChatHistoryRepository chatHistoryRepository,
            DiscordSocketClient discordSocketClient, 
            OpenAIChatService openAIChatService,
            TextToSpeechApiService textToSpeechApiService)
        {
            _logger = logger;

            _chatHistoryRepository = chatHistoryRepository;
            _discordSocketClient = discordSocketClient;
            _openAIChatService = openAIChatService;
            _textToSpeechApiService = textToSpeechApiService;
        }

        public override async Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            var message = notification.Message;
            if (message.Author.IsBot)
            {
                return;
            }

            var botUser = _discordSocketClient.CurrentUser;
            if (!message.MentionedUsers.Any(mu => mu.Id == botUser.Id))
            {
                return;
            }

            ChatMessage chatReply;
            using (message.Channel.EnterTypingState())
            {
                var usersInChannel = await message.Channel.GetUsersAsync().FlattenAsync();
                var channelUserDisplayNames = string.Join(',', usersInChannel.Where(u => !u.IsBot).Select(u => $"\"{((SocketGuildUser)u).DisplayName}\""));

                var chat = new List<ChatMessage>();

                const string systemMessageTemplate = "Гравці в чаті: {0}";
                var systemMessage = new ChatMessage(ChatRole.System, string.Format(systemMessageTemplate, channelUserDisplayNames));
                chat.Add(systemMessage);

                var chatHistory = await _chatHistoryRepository.GetChatHistory(message.Author.Id.ToString(), message.Channel.Id.ToString());
                chat.AddRange(chatHistory);

                var messageAuthorDisplayName = ((SocketGuildUser)message.Author).DisplayName;
                var messageCleanContent = message.CleanContent.Replace($"@{botUser.Username}#{botUser.Discriminator}", string.Empty).Trim();

                const string userMessageTemplate = "[{0}]: {1}";
                var userMessage = new ChatMessage(ChatRole.User, string.Format(userMessageTemplate, messageAuthorDisplayName, messageCleanContent));
                chat.Add(userMessage);

                chatReply = await _openAIChatService.GenerateResponse(chat);

                await message.Channel.SendMessageAsync(
                    chatReply.Content,
                    messageReference: new MessageReference(message.Id),
                    flags: MessageFlags.SuppressNotification);

                await _chatHistoryRepository.AppendChatHistory(
                    message.Author.Id.ToString(), 
                    message.Channel.Id.ToString(), 
                    message.Id.ToString(), 
                    userMessage,
                    chatReply);

                _logger.LogInformation("Replied to discord message '{messageId}'", message.Id);
            }

            var voiceChannel = message.Channel as SocketVoiceChannel;
            if (voiceChannel != null)
            {
                await _textToSpeechApiService.InitiateTextToSpeech($"{voiceChannel.Guild.Id}_{voiceChannel.Id}_{message.Id}", chatReply.Content);

                _logger.LogInformation("Text-to-speech initiated for message '{messageId}'", message.Id);
            }
        }
    }
}
