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
  }
}