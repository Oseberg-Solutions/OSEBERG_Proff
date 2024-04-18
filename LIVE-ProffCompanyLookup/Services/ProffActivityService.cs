using Azure.Data.Tables;
using LIVE_ProffCompanyLookup.Infrastructure;

namespace LIVE_ProffCompanyLookup.Services;

public class ProffActivityService
{
  private readonly AzureTableStorageService _storageService;
  private TableEntity? _entity;

  public ProffActivityService(AzureTableStorageService storageService)
  {
    _storageService = storageService;
  }

  public async Task UpdateRequestCountAsync(string domain)
  {
    var monthYear = DateTime.UtcNow.ToString("yyyyMM");
    var rowKey = $"{domain}_{monthYear}";

    _entity = await _storageService.RetrieveEntityAsync("domain", rowKey);

    if (_entity != null)
    {
      var amountOfRequests = GetAmountOfRequests();
      amountOfRequests++;

      _entity["amount_of_request"] = amountOfRequests;
      _entity["last_requests"] = DateTime.UtcNow;
      await _storageService.UpsertEntityAsync(_entity);
    }
    else
    {
      var newEntity = new TableEntity("domain", rowKey);
      newEntity.Add("domain", domain);
      newEntity.Add("amount_of_request", 1);
      newEntity.Add("amount_of_request", DateTime.UtcNow);
      await _storageService.UpsertEntityAsync(newEntity);
    }
  }

  private int GetAmountOfRequests()
  {
    return _entity != null && _entity.TryGetValue("amount_of_request", out var value)
      ? Convert.ToInt32(value)
      : 0;
  }
}