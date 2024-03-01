using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace ProffCompanyLookupService.Services
{
  public class AzureTableStorageService
  {
    private static string _storageAccountTableName;
    private static readonly string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=proffwebstorage;AccountKey=LOMG727/G2kA7AbOFIMfEg7aiF+xYiDGVcL44yUb92F3FcZ2vpdEp5AprrTck6UA9VxU8NVR8GFv+ASto5GNzQ==;EndpointSuffix=core.windows.net";

    public AzureTableStorageService(string TableName)
    {
      _storageAccountTableName = TableName;
    }

    public async Task UpdateProffDomainsTable(string domain)
    {

      CloudTable table = GetCloudTableRefference();

      // Define the query to fetch rows based on the desired criteria
      TableQuery<DynamicTableEntity> tableQuery = new TableQuery<DynamicTableEntity>()
          .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, domain))
          .Take(1);

      // Execute the query and retrieve the matching entity
      TableQuerySegment<DynamicTableEntity> segment = await table.ExecuteQuerySegmentedAsync(tableQuery, null);
      DynamicTableEntity entity = segment.FirstOrDefault();

      if (entity != null)
      {
        var amountOfRequests = entity.Properties["amount_of_request"].Int32Value ?? 0;
        amountOfRequests++;
        entity.Properties["amount_of_request"] = new EntityProperty(amountOfRequests);
        entity.Properties["last_request"] = new EntityProperty(DateTime.UtcNow);
        entity.Properties["Name"] = new EntityProperty(domain);

        // Save the updated entity back to the table
        TableOperation updateOperation = TableOperation.Replace(entity);
        await table.ExecuteAsync(updateOperation);
      }
      else
      {
        // Create a new entity with the domain as RowKey
        var newEntity = new DynamicTableEntity(domain, domain);
        newEntity.Properties.Add("amount_of_request", new EntityProperty(1));

        // Insert the new entity into the table
        TableOperation insertOperation = TableOperation.Insert(newEntity);
        await table.ExecuteAsync(insertOperation);
      }
    }

    public async Task<string> GetPremiumInfo(int orgNr)
    {
      CloudTable table = GetCloudTableRefference();
      List<string> columns = new List<string>() { "economy", "organisationNumber" };
      TableQuery<DynamicTableEntity> tableQuery = new TableQuery<DynamicTableEntity>()
        .Select(columns)
        .Where(TableQuery.GenerateFilterConditionForInt("organisationNumber", QueryComparisons.Equal, orgNr))
        .Take(1);

      TableQuerySegment<DynamicTableEntity> segment = await table.ExecuteQuerySegmentedAsync(tableQuery, null);

      DynamicTableEntity entity = segment.FirstOrDefault();

      if (entity != null)
      {
        var dictionary = entity.Properties.ToDictionary(
            prop => prop.Key,
            prop => prop.Value.PropertyAsObject
        );
        return JsonSerializer.Serialize(dictionary);
      }
      return "";
    }

    public CloudTable GetCloudTableRefference()
    {
      CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
      CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
      return tableClient.GetTableReference(_storageAccountTableName);
    }
  }
}
