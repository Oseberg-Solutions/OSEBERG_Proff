using System.Net;
using System.Security.Cryptography;
using System.Text;
using Proff.Infrastructure;
using Proff.Services;
using Proff.ExternalServices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Proff.Models;

namespace Proff.Function
{
  public class ProffCompanyLookup
  {
    private readonly ILogger<ProffCompanyLookup> _logger;
    private const string AzureRequestTableActivityName = "ProffRequestActivity";
    private const string AzureConfigurationTableName = "ProffConfiguration";
    private static HttpResponseData _response;


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
      _proffApiService = new ProffApiService();
    }

    [Function("ProffCompanyLookup")]
    public async Task<HttpResponseData> Run(
      [HttpTrigger(AuthorizationLevel.Function, "get", "post")]
      HttpRequestData req)
    {
      string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
      InputParams inputParams = new(req);

      if (!await EntityHasActiveSubscription(inputParams.domain))
      {
        return await ConstructHttpResponse(req, HttpStatusCode.BadRequest, "No active subscription found");
      }

      if (string.IsNullOrEmpty(inputParams.organisationNumber))
      {
        if (string.IsNullOrEmpty(inputParams.query) || string.IsNullOrEmpty(inputParams.country))
        {
          return await ConstructHttpResponse(req, HttpStatusCode.BadRequest, "Missing required parameters");
        }

        var companies = await GetCompanyData(inputParams.query, inputParams.country);
        await _proffActivityService.UpdateRequestCountAsync(inputParams.domain);
        return await ConstructHttpResponse(req, HttpStatusCode.OK, companies);
      }

      JObject extraCompanyInfo =
        await _proffApiService.GetDetailedCompanyInfoCopy(inputParams.country, inputParams.organisationNumber);
      await _proffActivityService.UpdateRequestCountAsync(inputParams.domain);
      return await ConstructHttpResponse(req, HttpStatusCode.OK, extraCompanyInfo);
    }

    private async Task<bool> EntityHasActiveSubscription(string domain)
    {
      var entity = await _azureConfigurationService.RetrieveEntityAsync(domain, domain);
      return entity != null && entity.GetBoolean("active_subscription") == true;
    }

    private async Task<List<CompanyData>> GetCompanyData(string query, string country)
    {
      JArray companies = await _proffApiService.FetchCompanyDataAsync(query, country);
      CompanyDataService companyDataService = new();
      var companyDataList = companyDataService.ConvertJArrayToCompanyDataList(companies);
      return companyDataList;
    }

    private async Task<HttpResponseData> ConstructHttpResponse(HttpRequestData req, HttpStatusCode statusCode,
      JObject extraCompanyInfo)
    {
      _response = req.CreateResponse(statusCode);
      string jsonString = JsonConvert.SerializeObject(extraCompanyInfo);
      _response.Headers.Add("Content-Type", "application/json; charset=utf-8");
      await _response.WriteStringAsync(jsonString);
      return _response;
    }

    private async Task<HttpResponseData> ConstructHttpResponse(HttpRequestData req, HttpStatusCode statusCode,
      string message)
    {
      _response = req.CreateResponse(statusCode);
      _response.Headers.Add("Content-Type", "application/json; charset=utf-8");
      await _response.WriteStringAsync(message);
      return _response;
    }

    private async Task<HttpResponseData> ConstructHttpResponse(HttpRequestData req, HttpStatusCode statusCode,
      List<CompanyData> companyData)
    {
      _response = req.CreateResponse(statusCode);
      string jsonString = JsonConvert.SerializeObject(companyData);
      _response.Headers.Add("Content-Type", "application/json; charset=utf-8");
      await _response.WriteStringAsync(jsonString);
      return _response;
    }
  }
}