using Microsoft.WindowsAzure.Storage.Table;
using ProffCompanyLookupService.Infrastructure;
using System;
using System.Threading.Tasks;

namespace ProffCompanyLookupService.Services
{
    public class ProffActivityService
  {
    private readonly AzureTableStorageService _tableService;

    public ProffActivityService(AzureTableStorageService tableService)
    {
      _tableService = tableService;
    }

    public async Task UpdateRequestCountAsync(string domain)
    {
      var entity = await _tableService.RetrieveEntityAsync(domain, domain);
      if (entity != null)
      {
        // Increment the request count
        var amountOfRequests = entity.Properties.ContainsKey("amount_of_request") ? entity.Properties["amount_of_request"].Int32Value ?? 0 : 0;
        amountOfRequests++;
        entity.Properties["amount_of_request"] = new EntityProperty(amountOfRequests);
        entity.Properties["last_request"] = new EntityProperty(DateTime.UtcNow);

        await _tableService.UpdateEntityAsync(entity);
      }
      else
      {
        DynamicTableEntity newEntity = new DynamicTableEntity(domain, domain);
        newEntity.Properties.Add("amount_of_request", new EntityProperty(1));
        newEntity.Properties.Add("domain", new EntityProperty(domain));
        newEntity.Properties.Add("last_request", new EntityProperty(DateTime.UtcNow));

        await _tableService.InsertOrMergeEntityAsync(newEntity);
      }
    }
  }
}
