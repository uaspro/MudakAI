﻿using Azure;
using Azure.Data.Tables;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using MudakAI.Connectors.Azure.Table;

namespace MudakAI.Chat.WebService.Repositories
{
    public class ChatHistoryEntity : ITableEntity
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public string Role { get; set; }

        public string Content { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }

    public class ChatHistoryRepository : TableStorageService<ChatHistoryEntity>
    {
        private readonly int _maxHistoryDepth;
        private readonly TimeSpan _maxHistoryAge;

        public ChatHistoryRepository(TableServiceClient tableServiceClient, int maxHistoryDepth, TimeSpan maxHistoryAge) : base(tableServiceClient)
        {
            _maxHistoryDepth = maxHistoryDepth;
            _maxHistoryAge = maxHistoryAge;
        }

        public override string TableName => "ChatHistory";

        public async Task AppendChatHistory(string userId, string channelId, string messageId, params ChatMessageContent[] chatHistory)
        {
            var now = DateTime.UtcNow;

            var chatHistoryEntities =
                chatHistory.Select((m, idx) => new ChatHistoryEntity
                {
                    PartitionKey = GetPartitionKey(userId, now),
                    RowKey = $"{channelId}_{messageId}_{idx}",
                    Role = m.Role.ToString(),
                    Content = m.Content,
                    Timestamp = now
                });

            await Upsert(chatHistoryEntities);
        }

        public async Task<ChatHistory> GetChatHistory(string userId, string channelId)
        {
            var now = DateTime.UtcNow;

            var partitionKey = GetPartitionKey(userId, now);
            DateTimeOffset thresholdTime = now - _maxHistoryAge;

            var chatHistoryEntities =
                await Find(ch => ch.PartitionKey == partitionKey && ch.Timestamp >= thresholdTime)
                        .Where(ch => ch.RowKey.StartsWith(channelId))
                        .Take(_maxHistoryDepth)
                        .ToArrayAsync();

            var chatHistory = new ChatHistory();

            chatHistory.AddRange(chatHistoryEntities.OrderBy(e => e.Timestamp)
                                      .ThenBy(e => e.RowKey)
                                      .Select(e => new ChatMessageContent(new AuthorRole(e.Role), e.Content)));

            return chatHistory;
        }

        private string GetPartitionKey(string userId, DateTime date)
        {
            return $"{DateTime.MaxValue.Date.Ticks - date.Date.Ticks:d20}_{userId}";
        }
    }
}
