using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ProffCompanyLookupService.Services;

namespace ProffCompanyLookupService.Functions
{
  public static class ProffCompanySearch
  {
    [FunctionName("ProffCompanySearch")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
      try
      {
        string domain = string.IsNullOrEmpty(req.Query["domain"]) ? "Unknown" : req.Query["domain"];
        string query = req.Query["query"];
        string country = req.Query["country"];
        string proffCompanyId = req.Query["proffCompanyId"];

        ProffApiService proffApiService = new();

        if (string.IsNullOrEmpty(proffCompanyId))
        {
          if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(country))
          {
            return new BadRequestObjectResult("Missing required parameters");
          }

          JArray companies = await proffApiService.FetchCompanyDataAsync(query, country);

          CompanyDataService companyDataService = new();
          var extractedData = companyDataService.ConvertJArrayToCompanyDataList(companies);
          await SaveToTable(domain);

          return new OkObjectResult(extractedData);
        }

        /*
         * This part will always happen when the user chooses a Company, and only then will this part of the code run, 
         * where we fetch detailed info where we have to grab from a different endpoint,
         * then the one in FetchCompanyDataAsync method on the proffApiService class.
        */

        (string numberOfEmployees,
         string nace,
         string profit,
         string revenue,
         string visitorAddressLine,
         string visitorBoxAddressLine,
         string visitorPostPlace,
         string visitorZipCode) = await proffApiService.GetDetailedCompanyInfo(country, proffCompanyId, log);

        JObject extraCompanyInfo = new()
        {
          ["numberOfEmployees"] = numberOfEmployees,
          ["Nace"] = nace,
          ["profit"] = profit,
          ["revenue"] = revenue,
          ["visitorAddressLine"] = visitorAddressLine,
          ["visitorBoxAddressLine"] = visitorBoxAddressLine,
          ["visitorPostPlace"] = visitorPostPlace,
          ["visitorZipCode"] = visitorZipCode
        };

        await SaveToTable(domain);

        return new OkObjectResult(extraCompanyInfo);

      }
      catch (Exception ex)
      {
        log.LogError(ex, "Error occurred during function execution.");
        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
      }
    }
    public static async Task SaveToTable(string domain)
    {
#if !DEBUG
      AzureTableStorageService tableStorageService = new();
      await tableStorageService.UpdateProffDomainsTable(domain);
#endif
    }
  }
}
