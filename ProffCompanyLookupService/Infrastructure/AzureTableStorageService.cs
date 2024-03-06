using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace ProffCompanyLookupService.Infrastructure
{
  public class AzureTableStorageService
  {
    private readonly CloudTable _table;
    private readonly string connectionString;
    private readonly string AZURE_PROFF_WEBSTORAGE_CONNECTIONSTRING = "AZURE_PROFF_WEBSTORAGE_CONNECTIONSTRING";

    public AzureTableStorageService(string tableName)
    {
      connectionString = Environment.GetEnvironmentVariable(AZURE_PROFF_WEBSTORAGE_CONNECTIONSTRING);
      CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
      CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
      _table = tableClient.GetTableReference(tableName);
    }

    public async Task InsertOrMergeEntityAsync(DynamicTableEntity entity)
    {
      if (entity == null)
      {
        throw new ArgumentNullException(nameof(entity));
      }
      TableOperation operation = TableOperation.InsertOrMerge(entity);
      await _table.ExecuteAsync(operation);
    }

    public async Task<DynamicTableEntity> RetrieveEntityAsync(string partitionKey, string rowKey)
    {
      TableOperation operation = TableOperation.Retrieve<DynamicTableEntity>(partitionKey, rowKey);
      TableResult result = await _table.ExecuteAsync(operation);
      return result.Result as DynamicTableEntity;
    }

    public async Task<IEnumerable<DynamicTableEntity>> QueryEntitiesAsync(string filter)
    {
      var query = new TableQuery<DynamicTableEntity>().Where(filter);
      TableQuerySegment<DynamicTableEntity> segment = await _table.ExecuteQuerySegmentedAsync(query, null);
      return segment.Results;
    }

    public async Task UpdateEntityAsync(DynamicTableEntity entity)
    {
      TableOperation operation = TableOperation.Replace(entity);
      await _table.ExecuteAsync(operation);
    }
  }
}
