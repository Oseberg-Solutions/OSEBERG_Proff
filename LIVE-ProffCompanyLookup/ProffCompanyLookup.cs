using System.Net;
using System.Text.Json;
using Proff.Infrastructure;
using Proff.Services;
using Proff.ExternalServices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Proff.Models;

namespace Proff.Function
{
  public class ProffCompanyLookup
  {
    private readonly ILogger<ProffCompanyLookup> _logger;
    private const string AzureStorageAccountTableName = "ProffRequestActivity";
    private static HttpResponseData _response;

    private AzureTableStorageService _azureStorageService;
    private ProffActivityService _proffActivityService;
    private ProffApiService _proffApiService;

    public ProffCompanyLookup(ILogger<ProffCompanyLookup> logger)
    {
      _logger = logger;
      _azureStorageService = new AzureTableStorageService(AzureStorageAccountTableName);
      _proffActivityService = new ProffActivityService(_azureStorageService);
      _proffApiService = new ProffApiService();
    }

    [Function("ProffCompanyLookup")]
    public async Task<HttpResponseData> Run(
      [HttpTrigger(AuthorizationLevel.Function, "get", "post")]
      HttpRequestData req)
    {
      string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
      ParamPayload paramPayLoad = new(req);

      if (string.IsNullOrEmpty(paramPayLoad.proffCompanyId))
      {
        if (string.IsNullOrEmpty(paramPayLoad.query) || string.IsNullOrEmpty(paramPayLoad.country))
        {
          return await ConstructHttpResponse(req, HttpStatusCode.BadRequest, "Missing required parameters");
        }

        var companies = GetCompanyData(paramPayLoad.query, paramPayLoad.country);
        await SaveToTable(paramPayLoad.domain);
        return await ConstructHttpResponse(req, HttpStatusCode.OK, companies);
      }

      JObject extraCompanyInfo =
        await _proffApiService.GetDetailedCompanyInfo(paramPayLoad.country, paramPayLoad.proffCompanyId);
      await _proffActivityService.UpdateRequestCountAsync(paramPayLoad.domain);
      return await ConstructHttpResponse(req, HttpStatusCode.OK, extraCompanyInfo);
    }

    private async Task<List<CompanyData>> GetCompanyData(string query, string country)
    {
      JArray companies = await _proffApiService.FetchCompanyDataAsync(query, country);
      CompanyDataService companyDataService = new();
      return companyDataService.ConvertJArrayToCompanyDataList(companies);
    }

    private async Task SaveToTable(string domain)
    {
      AzureTableStorageService tableService = new(AzureStorageAccountTableName);
      ProffActivityService proffActivityService = new(tableService);
      await proffActivityService.UpdateRequestCountAsync(domain);
    }

    private async Task<HttpResponseData> ConstructHttpResponse(HttpRequestData req, HttpStatusCode statusCode,
      JObject extraCompanyInfo)
    {
      _response = req.CreateResponse(statusCode);
      await _response.WriteAsJsonAsync(extraCompanyInfo);
      return _response;
    }

    private async Task<HttpResponseData> ConstructHttpResponse(HttpRequestData req, HttpStatusCode statusCode,
      string message)
    {
      _response = req.CreateResponse(statusCode);
      await _response.WriteAsJsonAsync(message);
      return _response;
    }

    private async Task<HttpResponseData> ConstructHttpResponse(HttpRequestData req, HttpStatusCode statusCode,
      Task<List<CompanyData>> companyData)
    {
      _response = req.CreateResponse(statusCode);
      await _response.WriteAsJsonAsync(companyData);
      return _response;
    }
  }

  public class Company
  {
    public string Name { get; set; }
    public string Description { get; set; }
  }
}