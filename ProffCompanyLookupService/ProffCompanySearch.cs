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
        ProffApiService proffApiService = new();
        string domain = string.IsNullOrEmpty(req.Query["domain"]) ? "Unknown" : req.Query["domain"];
        string query = req.Query["query"];
        string country = req.Query["country"];
        string proffCompanyId = req.Query["proffCompanyId"];


        if (string.IsNullOrEmpty(proffCompanyId))
        {
          if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(country))
          {
            return new BadRequestObjectResult("Missing required parameters");
          }

          JArray companies = await proffApiService.FetchCompanyDataAsync(query, country);

          CompanyDataService companyDataService = new CompanyDataService();
          var extractedData = companyDataService.ConvertJArrayToCompanyDataList(companies);

          return new OkObjectResult(extractedData);
        }

        /*
         * This part will always happen when the user chooses a Company, and only then will this part of the code run, 
         * where we fetch detailed info where we have to grab from a different endpoint then the one in FetchCompanyDataAsync method on the proffApiService class.
        */

        (string numberOfEmployees, string nace) = await proffApiService.GetDetailedCompanyInfo(country, proffCompanyId);

        JObject extraCompanyInfo = new()
        {
          ["numberOfEmployees"] = numberOfEmployees,
          ["Nace"] = nace
        };

        return new OkObjectResult(extraCompanyInfo);


#if !DEBUG
        AzureTableStorageService tableStorageService = new();
        await tableStorageService.UpdateProffDomainsTable(domain);
#endif

      }
      catch (Exception ex)
      {
        log.LogError(ex, "Error occurred during function execution.");
        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
      }
    }
  }
}
