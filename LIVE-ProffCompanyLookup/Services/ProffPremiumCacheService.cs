using Azure.Data.Tables;
using Proff.Models;
using Proff.Infrastructure;

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
      TableEntity newEntitty = new TableEntity(countryCode, orgNr);


      newEntitty.Add("economy", creditRating.Economy);
      newEntitty.Add("leadOwnership", creditRating.LeadOwnership);
      newEntitty.Add("organisationNumber", creditRating.OrganisationNumber.ToString());
      newEntitty.Add("rating", creditRating.Rating);
      newEntitty.Add("ratingScore", creditRating.RatingScore);

      await _storageService.UpsertEntityAsync(newEntitty);
    }
  }
}