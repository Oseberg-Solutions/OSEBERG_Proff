using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ProffCompanyLookupService.Models;
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

        if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(country))
        {
          return new BadRequestObjectResult("Missing required parameters");
        }

        ProffApiService proffApiService = new ProffApiService();
        JArray companies = await proffApiService.FetchCompanyDataAsync(query, country);

        CompanyDataService companyDataService = new CompanyDataService();
        var extractedData = companyDataService.ConvertJArrayToCompanyDataList(companies);

#if !DEBUG
                TableStorageService tableStorageService = new TableStorageService();
                await tableStorageService.UpdateProffDomainsTable(domain);
#endif

        return new OkObjectResult(extractedData);
      }
      catch (Exception ex)
      {
        log.LogError(ex, "Error occurred during function execution.");
        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
      }
    }
  }
}
