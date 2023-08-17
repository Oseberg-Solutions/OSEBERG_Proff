using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

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

        /*
          Todo:
          
        Okei, so when a a user search for a ORGNR, they will get all companies back if there is more then one.
        Before we give the result back to the user, we will allready find the ProffCompanyId adn find the nace and number of employees before retuning back the result.

         */

        // lets get the number of employees and nace for each Companies fetched
        // This is actually not IDEAL, becasue its overuse of API calls.
        // We should actually only do this when the user clicks on a Card, then we fetch the Data and send it back.
        foreach (var company in extractedData)
        {
          var details = await proffApiService.GetDetailedCompanyInfo(country, company.ProffCompanyId);
          company.NumberOfEmployees = details.NumberOfEmployees;
          company.Nace = details.Nace;
        }


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
