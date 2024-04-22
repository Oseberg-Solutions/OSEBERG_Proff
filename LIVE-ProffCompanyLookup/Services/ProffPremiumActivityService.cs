using Azure.Data.Tables;
using Proff.Infrastructure;

namespace Proff.Services
{
  public class ProffPremiumActivityService
  {
    private readonly AzureTableStorageService _storageService;
    private TableEntity? _entity;

    public ProffPremiumActivityService(AzureTableStorageService storageService)
    {
      _storageService = storageService;
    }

    public async Task UpdateRequestCountAsync(string origin)
    {
      var monthYear = DateTime.UtcNow.ToString("yyyyMM");
      var rowKey = $"{origin}_{monthYear}";

      _entity = await _storageService.RetrieveEntityAsync("domain", rowKey);

      if (_entity != null)
      {
        await UpdateExistingEntity();
      }
      else
      {
        await CreateNewEntity(rowKey, origin);
      }
    }

    private async Task UpdateExistingEntity()
    {
      var amountOfRequests = GetAmountOfRequests();
      amountOfRequests++;

      _entity["amount_of_request"] = amountOfRequests;
      _entity["last_request"] = DateTime.UtcNow;
      await _storageService.UpsertEntityAsync(_entity);
    }

    private async Task CreateNewEntity(string rowKey, string origin)
    {
      var newEntity = new TableEntity("domain", rowKey);
      newEntity.Add("domain", origin);
      newEntity.Add("amount_of_request", 1);
      newEntity.Add("last_request", DateTime.UtcNow);
      await _storageService.UpsertEntityAsync(newEntity);
    }

    private int GetAmountOfRequests()
    {
      return _entity != null && _entity.TryGetValue("amount_of_request", out var value)
        ? Convert.ToInt32(value)
        : 0;
    }
  }
}