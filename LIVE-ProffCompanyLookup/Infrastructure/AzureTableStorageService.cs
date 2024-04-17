using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Azure.Data.Tables;
using System.Collections.Generic;
using Azure.Data.Tables.Models;
using static Grpc.Core.Metadata;

namespace Proff.Infrastructure
{
  public class AzureTableStorageService
  {
    //private readonly CloudTable _table;
    private readonly string connectionString;
    private readonly string AZURE_PROFF_WEBSTORAGE_CONNECTIONSTRING = "AZURE_PROFF_WEBSTORAGE_CONNECTIONSTRING";
    private readonly string AZURE_PROFF_ACCOUNT_KEY = "AZURE_PROFF_ACCOUNT_KEY";

    public AzureTableStorageService(string tableName)
    {
      connectionString = Environment.GetEnvironmentVariable(AZURE_PROFF_WEBSTORAGE_CONNECTIONSTRING);
      var accountKey = Environment.GetEnvironmentVariable(AZURE_PROFF_ACCOUNT_KEY);
      //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

      var serviceClient = new TableServiceClient(connectionString);
      var tableClient = serviceClient.GetTableClient(tableName);

      var entity = tableClient.GetEntityIfExists<TableEntity>("domain", "rkf.crm4.dynamics.com_202403");
      var amountOfRequests = entity.Value.ContainsKey("amount_of_request") ? Convert.ToInt32(entity.Value["amount_of_request"]) : 0;

      var tableEntity = new TableEntity("domain", "suran.localhost202404")
      {
        { "amount_of_request", "Marker Set" },
        { "Price", 5.00 },
        { "Quantity", 21 },
        { "Quantity", 21 }

      };


      Console.WriteLine(tableClient.Uri);

      // we got to test this above.


      // _table = tableClient.GetTableReference(tableName);
    }
    /*

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
    */
  }
}
