using Azure;
using Azure.Data.Tables;
using MudakAI.Connectors.Azure.Table;
using System.Text.Json;

namespace MudakAI.Chat.WebService.Repositories
{
    public class BotConfigurationEntity : ITableEntity
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public string Key => RowKey;

        public string Value { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }

    public class BotConfigurationRepository : TableStorageService<BotConfigurationEntity>
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            IncludeFields = true
        };

        private const string PersonaKey = "Persona";

        public BotConfigurationRepository(TableServiceClient tableServiceClient) : base(tableServiceClient)
        {
        }

        public override string TableName => "BotConfig";

        public async Task<(string Description, string Voice)> GetPersona(string guildId)
        {
            var response = await Get(guildId, PersonaKey);
            return response != null 
                ? JsonSerializer.Deserialize<(string Description, string Voice)>(response.Value, JsonSerializerOptions)
                : (null, null);
        }

        public async Task SetPersona(string guildId, (string Description, string Voice) value)
        {
            await Upsert(
                new BotConfigurationEntity
                {
                    PartitionKey = guildId,
                    RowKey = PersonaKey,
                    Value = JsonSerializer.Serialize(value, JsonSerializerOptions)
                });
        }
    }
}
