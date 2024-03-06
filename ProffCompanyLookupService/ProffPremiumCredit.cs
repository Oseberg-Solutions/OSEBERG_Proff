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
using System;
using ProffCompanyLookupService.Utils;
using ProffCompanyLookupService.ExternalServices;
using ProffCompanyLookupService.Infrastructure;

namespace ProffCompanyLookupService
{
    public static class ProffPremiumCredit
  {
    private readonly static string _proffPremiumRequestActivity = "ProffPremiumRequestActivity";
    private readonly static string _proffPremiumCache = "ProffPremiumCache";
    private static ProffPremiumCacheService _proffPremiumCacheService;
    private static string _orgNr;
    private static string _countryCode;
    private static bool isValid;
    private static string validationMessage;

    [FunctionName("ProffPremiumCredit")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
      string orgNr = req.Query["orgNr"];
      _countryCode = StringValues.IsNullOrEmpty(req.Query["countryCode"]) ? "NO" : req.Query["countryCode"].ToString();

      (isValid, _orgNr, validationMessage) = ValidationUtils.ValidateAndFormatOrgNr(orgNr);
      if (!isValid) return new BadRequestObjectResult(validationMessage);

      _proffPremiumCacheService = InitializePremiumServices(_proffPremiumCache);
      var existingData = await _proffPremiumCacheService.GetPremiumInfoAsync(_orgNr, _countryCode);

      if (existingData != null)
      {
        await Console.Out.WriteLineAsync("Existing Data is not null");
        return new OkObjectResult(existingData);
      }

      ProffPremiumApiService proffPremiumApiService = new ProffPremiumApiService();
      var (creditRating, statusCode) = await proffPremiumApiService.GetCreditScore(_orgNr);

      return HandleResponse(creditRating, statusCode);
    }

    private static ProffPremiumCacheService InitializePremiumServices(string tableName)
    {
      AzureTableStorageService azureTableStorageService = new(tableName);
      ProffPremiumCacheService premiumService = new(azureTableStorageService);
      return premiumService;
    }

    private static IActionResult HandleResponse(CreditRating creditRating, HttpStatusCode statusCode)
    {
      switch (statusCode)
      {
        case HttpStatusCode.OK:
          _ = _proffPremiumCacheService.CreateOrUpdatePremiumInfoAsync(_orgNr, _countryCode, creditRating);
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