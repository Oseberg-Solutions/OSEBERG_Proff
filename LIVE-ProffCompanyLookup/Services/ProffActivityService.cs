﻿using Azure.Data.Tables;
using Proff.Infrastructure;

namespace Proff.Services;

public class ProffActivityService
{
  private readonly AzureTableStorageService _storageService;
  private TableEntity? _entity;
  private int _currentYear;
  private int _currentMonth;

  public ProffActivityService(AzureTableStorageService storageService)
  {
    _currentYear = DateTime.Today.Year;
    _currentMonth = DateTime.Today.Month;
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
    _entity["year"] = _currentYear;
    _entity["month"] = _currentMonth;
    await _storageService.UpsertEntityAsync(_entity);
  }

  private async Task CreateNewEntity(string rowKey, string origin)
  {
    var newEntity = new TableEntity("domain", rowKey)
    {
        { "domain", origin },
        { "amount_of_request", 1 },
        { "last_request", DateTime.UtcNow },
        { "year", _currentYear },
        { "month", _currentMonth }
    };
    await _storageService.UpsertEntityAsync(newEntity);
  }

  private int GetAmountOfRequests()
  {
    return _entity != null && _entity.TryGetValue("amount_of_request", out var value)
      ? Convert.ToInt32(value)
      : 0;
  }
}