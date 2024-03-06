using Microsoft.WindowsAzure.Storage.Table;
using ProffCompanyLookupService.Infrastructure;
using System.Threading.Tasks;
using System;

public class ProffActivityService
{
  private readonly AzureTableStorageService _tableService;

  public ProffActivityService(AzureTableStorageService tableService)
  {
    _tableService = tableService;
  }

  public async Task UpdateRequestCountAsync(string domain)
  {
    var monthYear = DateTime.UtcNow.ToString("yyyyMM");
    var rowKey = $"{domain}_{monthYear}";

    var entity = await _tableService.RetrieveEntityAsync("domain", rowKey);
    if (entity != null)
    {
      var amountOfRequests = entity.Properties.ContainsKey("amount_of_request") ? entity.Properties["amount_of_request"].Int32Value ?? 0 : 0;
      amountOfRequests++;
      entity.Properties["amount_of_request"] = new EntityProperty(amountOfRequests);
      entity.Properties["last_request"] = new EntityProperty(DateTime.UtcNow);

      await _tableService.UpdateEntityAsync(entity);
    }
    else
    {
      DynamicTableEntity newEntity = new DynamicTableEntity("domain", rowKey);
      newEntity.Properties.Add("domain", new EntityProperty(domain));
      newEntity.Properties.Add("amount_of_request", new EntityProperty(1));
      newEntity.Properties.Add("last_request", new EntityProperty(DateTime.UtcNow));

      await _tableService.InsertOrMergeEntityAsync(newEntity);
    }
  }
}
