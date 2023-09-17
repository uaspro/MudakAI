using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MudakAI.Connectors.Azure.Table
{
    public abstract class TableStorageService<T> where T : class, ITableEntity
    {
        private const int MaxPerPage = 50;

        private readonly TableServiceClient _tableServiceClient;

        public TableStorageService(TableServiceClient tableServiceClient)
        {
            _tableServiceClient = tableServiceClient;
        }

        public abstract string TableName { get; }

        public async Task<T> Get(string partitionKey, string rowKey)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(TableName);
                await tableClient.CreateIfNotExistsAsync();

                var tableEntity = await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
                return tableEntity.Value;
            } 
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task Upsert(T entity)
        {
            var tableClient = _tableServiceClient.GetTableClient(TableName);
            await tableClient.CreateIfNotExistsAsync();

            await tableClient.UpsertEntityAsync(entity);
        }

        public async Task Upsert(IEnumerable<T> entities)
        {
            var tableClient = _tableServiceClient.GetTableClient(TableName);
            await tableClient.CreateIfNotExistsAsync();

            var insertEntitiesBatch = new List<TableTransactionAction>();

            insertEntitiesBatch.AddRange(entities.Select(e => new TableTransactionAction(TableTransactionActionType.UpsertReplace, e)));

            await tableClient.SubmitTransactionAsync(insertEntitiesBatch);
        }

        public async IAsyncEnumerable<T> Find(Expression<Func<T, bool>> filterExpression)
        {
            var tableClient = _tableServiceClient.GetTableClient(TableName);
            await tableClient.CreateIfNotExistsAsync();

            var queryResults = tableClient.QueryAsync(filterExpression, maxPerPage: MaxPerPage);
            
            await foreach(var page in queryResults.AsPages())
            {
                foreach(var entity in page.Values)
                {
                    yield return entity;
                }
            }
        }
    }
}
