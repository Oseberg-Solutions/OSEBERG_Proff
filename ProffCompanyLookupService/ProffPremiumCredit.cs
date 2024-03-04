using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProffCompanyLookupService.Services;
using ProffCompanyLookupService.Models;
using System.Net;
using Microsoft.Extensions.Primitives;

namespace ProffCompanyLookupService
{
  public static class ProffPremiumCredit
  {
    private static string _storageAccountTableName = "ProffPremiumRequestActivity";

    [FunctionName("ProffPremiumCredit")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
      string orgNr = req.Query["orgNr"];
      string countryCode = StringValues.IsNullOrEmpty(req.Query["countryCode"]) ? "NO" : req.Query["countryCode"].ToString();

      var (isValid, formattedOrgNr, validationMessage) = ValidationUtils.ValidateAndFormatOrgNr(orgNr);
      if (!isValid) return new BadRequestObjectResult(validationMessage);

      AzureTableStorageService azureTableStorageService = new(_storageAccountTableName);

      PremiumInfoService premiumService = new(azureTableStorageService);
      var existingData = await premiumService.GetPremiumInfoAsync(orgNr, countryCode);

      if (string.IsNullOrEmpty(existingData))
      {
        // We dont have this cached data, lets do a API Call.
        return new OkObjectResult("We did not find any matching record");
      }

      return new OkObjectResult(existingData);

      ProffPremiumApiService proffPremiumApiService = new ProffPremiumApiService();
      var (creditRating, isSuccess, statusCode) = await proffPremiumApiService.GetCreditScore(formattedOrgNr);

      return HandleResponse(creditRating, isSuccess, statusCode);
    }

    private static IActionResult HandleResponse(CreditRating creditRating, bool isSuccess, HttpStatusCode statusCode)
    {
      switch (statusCode)
      {
        case HttpStatusCode.OK:
          return new OkObjectResult(creditRating);
        case HttpStatusCode.NoContent:
          return new NotFoundObjectResult("Company record not found.");
        case HttpStatusCode.InternalServerError:
          return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        default:
          return new StatusCodeResult((int)statusCode);
      }
    }
  }
}