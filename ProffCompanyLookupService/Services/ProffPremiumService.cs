using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProffCompanyLookupService.Services
{
  public class PremiumInfoService
  {
    private readonly AzureTableStorageService _tableService;

    public PremiumInfoService(AzureTableStorageService tableService)
    {
      _tableService = tableService;
    }

    public async Task<string> GetPremiumInfoAsync(string orgNr, string countryCode)
    {
      DynamicTableEntity entity = await _tableService.RetrieveEntityAsync(countryCode, orgNr);

      if (entity != null)
      {
        var properties = entity.Properties
            .ToDictionary(prop => prop.Key, prop => prop.Value.PropertyAsObject);

        return JsonSerializer.Serialize(properties, new JsonSerializerOptions { WriteIndented = true });
      }
      return null;
    }
    public async Task CreateOrUpdatePremiumInfoAsync(string orgNr, Dictionary<string, object> properties)
    {
      // Note: Adjust PartitionKey and RowKey design based on your actual schema requirements
      DynamicTableEntity entity = new DynamicTableEntity(orgNr, orgNr);

      // Add properties from the provided dictionary to the entity
      foreach (var property in properties)
      {
        entity.Properties.Add(property.Key, EntityProperty.CreateEntityPropertyFromObject(property.Value));
      }

      // Use the AzureTableStorageService to insert or merge the entity into the table
      await _tableService.InsertOrMergeEntityAsync(entity);
    }
  }
}