using Azure.Data.Tables;
using Proff.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Proff.Services
{
  public class ProffPremiumCacheService
  {
    /*
    private readonly AzureTableStorageService _tableService;

    public ProffPremiumCacheService(AzureTableStorageService tableService)
    {
      _tableService = tableService;
    }

    public async Task<Dictionary<string, object>> GetPremiumInfoAsync(string orgNr, string countryCode)
    {
      DynamicTableEntity entity = await _tableService.RetrieveEntityAsync(countryCode, orgNr);

      if (entity != null)
      {
        var properties = entity.Properties
            .ToDictionary(prop => prop.Key, prop => prop.Value.PropertyAsObject);

        return properties;
      }
      return null;
    }

    public async Task CreateOrUpdatePremiumInfoAsync(string orgNr, string countryCode, CreditRating creditRating)
    {
      DynamicTableEntity entity = new(countryCode, orgNr);

      entity.Properties.Add("economy", new EntityProperty(creditRating.Economy));
      entity.Properties.Add("leadOwnership", new EntityProperty(creditRating.LeadOwnership));
      entity.Properties.Add("organisationNumber", new EntityProperty(creditRating.OrganisationNumber.ToString()));
      entity.Properties.Add("rating", new EntityProperty(creditRating.Rating));
      entity.Properties.Add("ratingScore", new EntityProperty(creditRating.RatingScore));

      await _tableService.InsertOrMergeEntityAsync(entity);
    }
    */
  }
}