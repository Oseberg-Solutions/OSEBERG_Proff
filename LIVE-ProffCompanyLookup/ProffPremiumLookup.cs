using System.Collections.Generic;
using System.Net;
using LIVE_ProffCompanyLookup.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Proff.ExternalServices;
using Proff.Infrastructure;
using Proff.Models;
using Proff.Services;

namespace Proff.Function;

public class ProffPremiumLookup
{
  private const string AzureConfigurationTableName = "ProffConfiguration";
  private const string HttpMessageMissingRequiredParameters = "Missing required parameters";
  private const string HttpMessageNoActivePremiumSubscription = "No active premium subscription found";
  private readonly static string _proffPremiumRequestActivityTableName = "ProffPremiumRequestActivity";
  private readonly static string _proffPremiumCacheTableName = "ProffPremiumCache";
  private readonly ILogger _logger;
  private static HttpResponseData? _response;
  private AzureTableStorageService _azureConfigurationService;
  private ProffPremiumApiService _proffPremiumApiService;

  public ProffPremiumLookup(ILoggerFactory loggerFactory)
  {
    _logger = loggerFactory.CreateLogger<ProffPremiumLookup>();
    _azureConfigurationService = new AzureTableStorageService(AzureConfigurationTableName);
    _proffPremiumApiService = new ProffPremiumApiService();
  }

  [Function("ProffPremiumLookup")]
  public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
    FunctionContext executionContext)
  {
    InputParams inputParams = new(req);
    if (inputParams.domain == null || inputParams.organisationNumber == null || inputParams.country == null)
    {
      return await HttpHelper.ConstructHttpResponse(_response, req, HttpStatusCode.BadRequest, HttpMessageMissingRequiredParameters);
    }

    if (!await _azureConfigurationService.DoesEntityHavePremiumLicenseAsync(inputParams.domain))
    {
      return await HttpHelper.ConstructHttpResponse(_response, req, HttpStatusCode.BadRequest, HttpMessageNoActivePremiumSubscription);
    }

    ProffPremiumCacheService _proffPremiumCacheService = InitializePremiumCacheService(_proffPremiumCacheTableName);
    var existingData = await _proffPremiumCacheService.GetPremiumInfoAsync(inputParams.organisationNumber, inputParams.country);

    if (existingData != null)
    {
      return await HttpHelper.ConstructHttpResponse(_response, req, HttpStatusCode.OK, JObject.FromObject(existingData));
    }
    else
    {
      var (creditRating, statusCode) = await _proffPremiumApiService.GetCreditScore(inputParams.organisationNumber);
      var proffPremiumActivityService = InitializePremiumActivityService(_proffPremiumRequestActivityTableName);
      await proffPremiumActivityService.UpdateRequestCountAsync(inputParams.organisationNumber);
      return await HttpHelper.ConstructHttpResponse(_response, req, statusCode, JObject.FromObject(creditRating));
    }
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
}