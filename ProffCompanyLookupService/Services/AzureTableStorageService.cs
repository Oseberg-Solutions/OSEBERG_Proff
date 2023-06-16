using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ProffCompanyLookupService.Services
{
  public class AzureTableStorageService
  {
    private static string storageAccountTableName = "ProffDomains";
    private static readonly string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=proffwebstorage;AccountKey=LOMG727/G2kA7AbOFIMfEg7aiF+xYiDGVcL44yUb92F3FcZ2vpdEp5AprrTck6UA9VxU8NVR8GFv+ASto5GNzQ==;EndpointSuffix=core.windows.net";

    public async Task UpdateProffDomainsTable(string domain)
    {
      // Get a reference to the Azure Table storage
      CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
      CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
      CloudTable table = tableClient.GetTableReference(storageAccountTableName);

      // Define the query to fetch rows based on the desired criteria
      TableQuery<DynamicTableEntity> tableQuery = new TableQuery<DynamicTableEntity>()
          .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, domain))
          .Take(1); // Fetch only one row

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
  }
}
