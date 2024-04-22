using Azure.Data.Tables;

namespace Proff.Infrastructure
{
  public class AzureTableStorageService
  {
    private readonly TableClient _tableClient;
    private readonly string _connectionString;
    private readonly string AZURE_PROFF_WEBSTORAGE_CONNECTIONSTRING = "AZURE_PROFF_WEBSTORAGE_CONNECTIONSTRING";
    private readonly string AZURE_PROFF_ACCOUNT_KEY = "AZURE_PROFF_ACCOUNT_KEY";

    public AzureTableStorageService(string tableName)
    {
      _connectionString = Environment.GetEnvironmentVariable(AZURE_PROFF_WEBSTORAGE_CONNECTIONSTRING);
      var accountKey = Environment.GetEnvironmentVariable(AZURE_PROFF_ACCOUNT_KEY);

      var serviceClient = new TableServiceClient(_connectionString);
      _tableClient = serviceClient.GetTableClient(tableName);
    }

    public async Task<TableEntity?> RetrieveEntityAsync(string partitionKey, string rowKey)
    {
      var entity = await _tableClient.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey);
      return entity.HasValue ? entity.Value : null;
    }

    public async Task UpsertEntityAsync(TableEntity entity)
    {
      ArgumentNullException.ThrowIfNull(entity);
      await _tableClient.UpsertEntityAsync(entity);
    }

    /*
public async Task<IEnumerable<DynamicTableEntity>> QueryEntitiesAsync(string filter)
{
  var query = new TableQuery<DynamicTableEntity>().Where(filter);
  TableQuerySegment<DynamicTableEntity> segment = await _table.ExecuteQuerySegmentedAsync(query, null);
  return segment.Results;
}
*/
  }
}