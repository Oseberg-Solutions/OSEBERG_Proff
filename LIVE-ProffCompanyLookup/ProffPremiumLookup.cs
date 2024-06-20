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
  private const string AzureRequestTableActivityName = "ProffRequestActivity";
  private const string AzureConfigurationTableName = "ProffConfiguration";
  private const string HttpMessageMissingRequiredParameters = "Missing required parameters";
  private const string HttpMessageNoActivePremiumSubscription = "No active premium subscription found";

  private readonly static string _proffPremiumRequestActivity = "ProffPremiumRequestActivity";
  private readonly static string _proffPremiumCache = "ProffPremiumCache";
  private static ProffPremiumCacheService _proffPremiumCacheService;
  private static ProffPremiumActivityService _proffPremiumActivityService;
  private readonly ILogger _logger;
  private static HttpResponseData? _response;
  private AzureTableStorageService _azureRequestActivityService;
  private AzureTableStorageService _azureConfigurationService;
  private ProffPremiumActivityService _proffActivityService;
  private ProffPremiumApiService _proffPremiumApiService;

  public ProffPremiumLookup(ILoggerFactory loggerFactory)
  {
    _logger = loggerFactory.CreateLogger<ProffPremiumLookup>();
    _azureRequestActivityService = new AzureTableStorageService(AzureRequestTableActivityName);
    _azureConfigurationService = new AzureTableStorageService(AzureConfigurationTableName);
    _proffActivityService = new ProffPremiumActivityService(_azureRequestActivityService);
    //_proffApiService = new ProffPremiumApiService(_azureConfigurationService);
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

    _proffPremiumCacheService = InitializePremiumCacheService(_proffPremiumCache);
    var existingData = await _proffPremiumCacheService.GetPremiumInfoAsync(inputParams.organisationNumber, inputParams.country);

    if (existingData != null)
    {
      return await HttpHelper.ConstructHttpResponse(_response, req, HttpStatusCode.OK, JObject.FromObject(existingData));
    }
    else
    {
      var (creditRating, statusCode) = await _proffPremiumApiService.GetCreditScore(inputParams.organisationNumber);
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