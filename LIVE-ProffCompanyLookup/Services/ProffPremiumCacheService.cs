using Azure.Data.Tables;
using Proff.Models;
using Proff.Infrastructure;
using Microsoft.AspNetCore.Routing;

namespace Proff.Services
{
  public class ProffPremiumCacheService
  {
    private readonly AzureTableStorageService _storageService;

    public ProffPremiumCacheService(AzureTableStorageService tableService)
    {
      _storageService = tableService;
    }

    public async Task<Dictionary<string, object>> GetPremiumInfoAsync(string orgNr, string countryCode)
    {
      TableEntity entity = await _storageService.RetrieveEntityAsync(countryCode, orgNr);

      if (entity != null)
      {
        var properties = entity.ToDictionary();
        return properties;
      }
      return null;
    }

    public async Task CreateOrUpdatePremiumInfoAsync(string orgNr, string countryCode, CreditRating creditRating)
    {
      TableEntity newEntitty = new(countryCode, orgNr)
      {
          { "economy", creditRating.Economy },
          { "leadOwnership", creditRating.LeadOwnership },
          { "organisationNumber", creditRating.OrganisationNumber.ToString() },
          { "rating", creditRating.Rating },
          { "ratingScore", creditRating.RatingScore }
      };

      await _storageService.UpsertEntityAsync(newEntitty);
    }

    // TODO: We have to test these 3 methods below. ClearOldTableEntries, GetOdlestEntityAsync, DeleteAllEntitiesAsync
    //We need a method to clear the table of old entries after 90 days.
    /*
    public async Task ClearOldTableEntriesAsync()
    {
      var oldestEntity = await GetOldestEntityAsync();
      System.Console.WriteLine("Oldest entity: " + oldestEntity.RowKey);
      if (oldestEntity != null)
      {
        DateTimeOffset oldestTimestamp = oldestEntity.Timestamp.GetValueOrDefault();
        if (DateTimeOffset.UtcNow - oldestTimestamp > TimeSpan.FromDays(90))
        {
          await DeleteAllEntitiesAsync();
        }
      }
    }

    private async Task<TableEntity> GetOldestEntityAsync()
    {
      var queryResult = _storageService.GetTableClient().QueryAsync<TableEntity>();
      await foreach (var entity in queryResult)
      {
        return entity;  // Return the first (oldest) entity found
      }
      return null;
    }

    private async Task DeleteAllEntitiesAsync()
    {
      var queryResult = _storageService.GetTableClient().QueryAsync<TableEntity>(filter: null);
      var deleteTasks = new List<Task>();
      await foreach (var entity in queryResult)
      {
        deleteTasks.Add(_storageService.GetTableClient().DeleteEntityAsync(entity.PartitionKey, entity.RowKey));
      }
      await Task.WhenAll(deleteTasks);  // Wait for all delete operations to complete
    }
    */

  }
}