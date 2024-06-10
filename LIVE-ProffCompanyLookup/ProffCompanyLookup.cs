using System.Net;
using Proff.Infrastructure;
using Proff.Services;
using Proff.ExternalServices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Proff.Models;
using LIVE_ProffCompanyLookup.Utils;

namespace Proff.Function
{
  public class ProffCompanyLookup
  {
    private readonly ILogger<ProffCompanyLookup> _logger;
    private const string AzureRequestTableActivityName = "ProffRequestActivity";
    private const string AzureConfigurationTableName = "ProffConfiguration";
    private const string HttpMessageMissingRequiredParameters = "Missing required parameters";
    private const string HttpMessageNoActiveSubscription = "No active subscription found";
    private static HttpResponseData? _response;
    private AzureTableStorageService _azureRequestActivityService;
    private AzureTableStorageService _azureConfigurationService;
    private ProffActivityService _proffActivityService;
    private ProffApiService _proffApiService;

    public ProffCompanyLookup(ILogger<ProffCompanyLookup> logger)
    {
      _logger = logger;
      _azureRequestActivityService = new AzureTableStorageService(AzureRequestTableActivityName);
      _azureConfigurationService = new AzureTableStorageService(AzureConfigurationTableName);
      _proffActivityService = new ProffActivityService(_azureRequestActivityService);
      _proffApiService = new ProffApiService(_azureConfigurationService);
    }

    [Function("ProffCompanyLookup")]
    public async Task<HttpResponseData> Run(
      [HttpTrigger(AuthorizationLevel.Function, "get")]
      HttpRequestData req, FunctionContext executionContext)
    {
<<<<<<< HEAD
<<<<<<< HEAD
      _logger.LogInformation("Starting...");
=======
>>>>>>> 9233d8f1701f3ef774ca6918b4192a8c6c909599
      string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
      InputParams inputParams = new(req);

      _logger.LogInformation(message: JsonConvert.SerializeObject(inputParams));

      if (!await EntityHasActiveSubscription(inputParams.domain))
=======
      InputParams inputParams = new InputParams(req);

      if (!await _azureConfigurationService.EntityHasActiveSubscription(inputParams.domain))
>>>>>>> a87e16a1e1ebee2c924c00e0814e625526ff97c4
      {
        return await HttpHelper.ConstructHttpResponse(_response, req, HttpStatusCode.BadRequest,
          HttpMessageNoActiveSubscription);
      }

      if (string.IsNullOrEmpty(inputParams.organisationNumber))
      {
        if (string.IsNullOrEmpty(inputParams.query) || string.IsNullOrEmpty(inputParams.country))
        {
          return await HttpHelper.ConstructHttpResponse(_response, req, HttpStatusCode.BadRequest,
            HttpMessageMissingRequiredParameters);
        }


        _logger.LogInformation("Get CompanyData...");

        var companies = await GetCompanyData(inputParams.query, inputParams.country);
        await _proffActivityService.UpdateRequestCountAsync(inputParams.domain);
        return await HttpHelper.ConstructHttpResponse(_response, req, HttpStatusCode.OK, companies);
      }

      _logger.LogInformation("Get Detailed Company Info");

      JObject extraCompanyInfo =
        await _proffApiService.GetDetailedCompanyInfoCopy(inputParams.country, inputParams.organisationNumber);
      await _proffActivityService.UpdateRequestCountAsync(inputParams.domain);
      return await HttpHelper.ConstructHttpResponse(_response, req, HttpStatusCode.OK, extraCompanyInfo);
    }

    private async Task<List<CompanyData>> GetCompanyData(string query, string country)
    {
      JArray companies = await _proffApiService.FetchCompanyDataAsync(query, country);
      CompanyDataService companyDataService = new();
      var companyDataList = companyDataService.ConvertJArrayToCompanyDataList(companies);
      return companyDataList;
    }
  }
}