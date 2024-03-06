using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Net;
using System;
using ProffCompanyLookupService.Services;
using ProffCompanyLookupService.Models;
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
    private static ProffPremiumActivityService _proffPremiumActivityService;
    private static string _orgNr;
    private static string _domain;
    private static string _countryCode;
    private static bool isValid;
    private static string _validationMessage;

    [FunctionName("ProffPremiumCredit")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
    {

      (isValid, _orgNr, _domain, _countryCode, _validationMessage) = ValidationUtils.ValidateQueryInput(req);
      if (!isValid) return new BadRequestObjectResult(_validationMessage);

      _proffPremiumCacheService = InitializePremiumCacheService(_proffPremiumCache);
      var existingData = await _proffPremiumCacheService.GetPremiumInfoAsync(_orgNr, _countryCode);

      if (existingData != null)
      {
        await Console.Out.WriteLineAsync("Existing Data is not null");
        return new OkObjectResult(existingData);
      }

      ProffPremiumApiService proffPremiumApiService = new();
      var (creditRating, statusCode) = await proffPremiumApiService.GetCreditScore(_orgNr);

      return HandleResponse(creditRating, statusCode);
    }

    private static ProffPremiumCacheService InitializePremiumCacheService(string tableName)
    {
      AzureTableStorageService azureTableStorageService = new(tableName);
      ProffPremiumCacheService premiumService = new(azureTableStorageService);
      return premiumService;
    }

    private static ProffPremiumActivityService InitializePremiumActivityService(string tableName)
    {
      AzureTableStorageService azureTableStorageService = new(tableName);
      ProffPremiumActivityService proffPremiumActivityService = new(azureTableStorageService);
      return proffPremiumActivityService;
    }

    private static IActionResult HandleResponse(CreditRating creditRating, HttpStatusCode statusCode)
    {
      switch (statusCode)
      {
        case HttpStatusCode.OK:
          _ = _proffPremiumCacheService.CreateOrUpdatePremiumInfoAsync(_orgNr, _countryCode, creditRating);
          _proffPremiumActivityService = InitializePremiumActivityService(_proffPremiumRequestActivity);
          _ = _proffPremiumActivityService.UpdateRequestCountAsync(_domain);

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