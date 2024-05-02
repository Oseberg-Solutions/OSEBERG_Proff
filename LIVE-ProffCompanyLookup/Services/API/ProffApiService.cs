using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Proff.Infrastructure;

namespace Proff.ExternalServices
{
  public class ProffApiService
  {
    private static readonly HttpClient httpClient = new();
    private readonly static string PROFF_BASE_URL = "https://api.proff.no/api";
    private readonly string _proffApiKey;
    private readonly AzureTableStorageService _azureProffConfigurationTableService;

    public ProffApiService(AzureTableStorageService azureProffConfigurationTableService)
    {
      _azureProffConfigurationTableService = azureProffConfigurationTableService;
      _proffApiKey = Environment.GetEnvironmentVariable("PROFF_API_KEY");
    }

    public async Task<JArray> FetchCompanyDataAsync(string query, string country)
    {
      string proffApiUrl = ContainsOnlyDigits(query)
        ? $"{PROFF_BASE_URL}/companies/eniropro/{country}?industry={query}"
        : $"{PROFF_BASE_URL}/companies/eniropro/{country}?name={query}";

      httpClient.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _proffApiKey);
      HttpResponseMessage response = await httpClient.GetAsync(proffApiUrl);
      string responseContent = await response.Content.ReadAsStringAsync();

      if (!response.IsSuccessStatusCode)
      {
        throw new Exception($"Error calling Proff API: {responseContent}");
      }

      JObject apiResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
      return CreateJArrayFromApiResponse(apiResponse);
    }

    public async Task<JObject> GetDetailedCompanyInfoCopy(string country, string organisationNumber)
    {
      string proffCompanyListingUrl = $"{PROFF_BASE_URL}/companies/register/{country}/{organisationNumber}";
      HttpResponseMessage response = await FetchProffDataAsync(proffCompanyListingUrl);

      if (!response.IsSuccessStatusCode)
      {
        throw await HandleProffApiErrorAsync(response);
      }

      string apiResponseContent = await response.Content.ReadAsStringAsync();
      JObject apiResponse = JObject.Parse(apiResponseContent);

      JObject companyInfo = await ExtractDetailedCompanyInfoAsync(apiResponse);

      return companyInfo;
    }

    private async Task<HttpResponseMessage> FetchProffDataAsync(string url)
    {
      httpClient.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _proffApiKey);
      return await httpClient.GetAsync(url);
    }

    private async Task<Exception> HandleProffApiErrorAsync(HttpResponseMessage response)
    {
      string responseContent = await response.Content.ReadAsStringAsync();
      string errorMessage = $"Error calling Proff API for detailed info: {responseContent}";
      return new Exception(errorMessage);
    }

    private async Task<JObject> ExtractDetailedCompanyInfoAsync(JObject apiResponse)
    {
      var jsonObject = new JObject
      {
        ["Nace"] = apiResponse["naceCategories"]?[0]?.ToString(),
        ["numberOfEmployees"] = apiResponse["numberOfEmployees"]?.ToString(),
        ["visitorAddressLine"] = apiResponse["visitorAddress"]?["addressLine"]?.ToString(),
        ["visitorBoxAddressLine"] = apiResponse["visitorAddress"]?["boxAddressLine"]?.ToString(),
        ["visitorPostPlace"] = apiResponse["visitorAddress"]?["postPlace"]?.ToString(),
        ["visitorZipCode"] = apiResponse["visitorAddress"]?["zipCode"]?.ToString(),
      };

      // Premium fields (if applicable)
      if (EntityPremiumLicenseIsActive())
      {
        jsonObject["likviditetsgrad"] =
          apiResponse["analyses"]?[0]?["companyFigures"]?["likviditetsgrad"]?.ToString();
        jsonObject["totalrentabilitetLoennsomhet"] =
          apiResponse["analyses"]?[0]?["companyFigures"]?["totalrentabilitetLoennsomhet"]?.ToString();
        jsonObject["egenkapitalandel"] =
          apiResponse["analyses"]?[0]?["companyFigures"]?["egenkapitalandel"]?.ToString();

        jsonObject["profit"] = apiResponse["profit"]?.ToString();
        jsonObject["revenue"] = apiResponse["revenue"]?.ToString();
      }

      return jsonObject;
    }

    private bool EntityPremiumLicenseIsActive()
    {
      return _azureProffConfigurationTableService.EntityHasPremiumLicense();
    }

    public async Task<JObject> GetDetailedCompanyInfo(string country, string proffCompanyId)
    {
      string proffCompanyListingUrl = $"{PROFF_BASE_URL}/companies/eniropro/{country}/{proffCompanyId}";

      httpClient.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _proffApiKey);
      HttpResponseMessage response = await httpClient.GetAsync(proffCompanyListingUrl);

      if (!response.IsSuccessStatusCode)
      {
        string responseContent = await response.Content.ReadAsStringAsync();
        throw new Exception($"Error calling Proff API for detailed info: {responseContent}");
      }

      string apiResponseContent = await response.Content.ReadAsStringAsync();

      JObject apiResponse = JObject.Parse(apiResponseContent);

      var jsonObject = new JObject()
      {
        ["Nace"] = apiResponse["registerListing"]["naceCategories"]?[0]?.ToString(),
        ["numberOfEmployees"] = apiResponse["registerListing"]["numberOfEmployees"]?.ToString(),
        ["visitorAddressLine"] = apiResponse["registerListing"]["visitorAddress"]["addressLine"].ToString(),
        ["visitorBoxAddressLine"] = apiResponse["registerListing"]["visitorAddress"]["boxAddressLine"].ToString(),
        ["visitorPostPlace"] = apiResponse["registerListing"]["visitorAddress"]["postPlace"].ToString(),
        ["visitorZipCode"] = apiResponse["registerListing"]["visitorAddress"]["zipCode"].ToString(),
      };

      if (EntityPremiumLicenseIsActive())
      {
        jsonObject["profit"] = apiResponse["registerListing"]["profit"]?.ToString();
        jsonObject["revenue"] = apiResponse["registerListing"]["revenue"]?.ToString();
      }

      return jsonObject;
    }

    private static JArray CreateJArrayFromApiResponse(JObject apiResponse)
    {
      var jsonArray = apiResponse.ContainsKey("companyTypeName")
        ? new JArray(apiResponse)
        : apiResponse["companies"] as JArray;
      return jsonArray;
    }

    private static bool ContainsOnlyDigits(string str)
    {
      return str.All(char.IsDigit);
    }
  }
}