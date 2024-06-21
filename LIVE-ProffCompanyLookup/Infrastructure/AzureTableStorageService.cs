using Azure.Data.Tables;
using static Grpc.Core.Metadata;

namespace Proff.Infrastructure
{
  public class AzureTableStorageService
  {
    private readonly TableClient _tableClient;
    private readonly string _connectionString;

    private TableEntity? _entity;

    public AzureTableStorageService(string tableName)
    {
      _connectionString = Environment.GetEnvironmentVariable("AZURE_PROFF_WEBSTORAGE_CONNECTIONSTRING");
      var serviceClient = new TableServiceClient(_connectionString);
      _tableClient = serviceClient.GetTableClient(tableName);
    }

    public async Task<TableEntity?> RetrieveEntityAsync(string partitionKey, string rowKey)
    {
      var entity = await _tableClient.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey);
      if (entity.HasValue)
        _entity = entity.Value;
      return _entity;
    }

    public async Task<bool> EntityHasActiveSubscription(string? domain)
    {
      var entity = await RetrieveEntityAsync(domain, domain);
      return entity != null && entity.GetBoolean("active_subscription") == true;
    }

    public async Task<bool> DoesEntityHavePremiumLicenseAsync(string? domain)
    {
      var entity = await RetrieveEntityAsync(domain, domain);
      return entity != null && entity.GetBoolean("premium_subscription") == true;
    }

    public async Task UpsertEntityAsync(TableEntity entity)
    {
      ArgumentNullException.ThrowIfNull(entity);
      await _tableClient.UpsertEntityAsync(entity);
    }

    public TableClient GetTableClient()
    {
      return _tableClient;
    }
  }
}