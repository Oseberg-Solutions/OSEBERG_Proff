using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Proff.ExternalServices
{
  public class ProffApiService
  {
    private static readonly HttpClient httpClient = new();
    private readonly static string PROFF_BASE_URL = "https://api.proff.no/api";
    private readonly string _proffApiKey;

    public ProffApiService()
    {
      _proffApiKey = Environment.GetEnvironmentVariable("PROFF_API_KEY");
    }

    public async Task<JArray> FetchCompanyDataAsync(string query, string country, ILogger log)
    {
      string proffApiUrl = ContainsOnlyDigits(query)
          ? $"{PROFF_BASE_URL}/companies/eniropro/{country}?industry={query}"
          : $"{PROFF_BASE_URL}/companies/eniropro/{country}?name={query}";

      httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _proffApiKey);
      HttpResponseMessage response = await httpClient.GetAsync(proffApiUrl);
      string responseContent = await response.Content.ReadAsStringAsync();

      if (!response.IsSuccessStatusCode)
      {
        throw new Exception($"Error calling Proff API: {responseContent}");
      }

      JObject apiResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
      await Console.Out.WriteLineAsync($"ApiResponse: {apiResponse}");

      return CreateJArrayFromApiResponse(apiResponse);
    }

    public async Task<JObject> GetDetailedCompanyInfo(string country, string proffCompanyId, ILogger log)
    {
      string proffCompanyListingUrl = $"{PROFF_BASE_URL}/companies/eniropro/{country}/{proffCompanyId}";

      httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _proffApiKey);
      HttpResponseMessage response = await httpClient.GetAsync(proffCompanyListingUrl);

      if (!response.IsSuccessStatusCode)
      {
        string responseContent = await response.Content.ReadAsStringAsync();
        throw new Exception($"Error calling Proff API for detailed info: {responseContent}");
      }

      string apiResponseContent = await response.Content.ReadAsStringAsync();

      JObject apiResponse = JObject.Parse(apiResponseContent);

      return new()
      {
        ["numberOfEmployees"] = apiResponse["registerListing"]["numberOfEmployees"]?.ToString(),
        ["Nace"] = apiResponse["registerListing"]["naceCategories"]?[0]?.ToString(),
        ["profit"] = apiResponse["registerListing"]["profit"]?.ToString(),
        ["revenue"] = apiResponse["registerListing"]["revenue"]?.ToString(),
        ["visitorAddressLine"] = apiResponse["registerListing"]["visitorAddress"]["addressLine"].ToString(),
        ["visitorBoxAddressLine"] = apiResponse["registerListing"]["visitorAddress"]["boxAddressLine"].ToString(),
        ["visitorPostPlace"] = apiResponse["registerListing"]["visitorAddress"]["postPlace"].ToString(),
        ["visitorZipCode"] = apiResponse["registerListing"]["visitorAddress"]["zipCode"].ToString(),
        /*
        ["likviditetsgrad"] = apiResponse["registerOwnListing"]["analyses"]["companyFigures"]["likviditetsgrad"]?.ToString(),
        ["totalrentabilitetLoennsomhet "] = apiResponse["registerOwnListing"]["analyses"]["companyFigures"]["totalrentabilitetLoennsomhet"]?.ToString(),
        ["egenkapitalandel "] = apiResponse["registerOwnListing"]["analyses"]["companyFigures"]["egenkapitalandel"]?.ToString(),
        */
      };
    }

    private static JArray CreateJArrayFromApiResponse(JObject apiResponse)
    {
      return apiResponse.ContainsKey("companyTypeName") ? new JArray(apiResponse) : apiResponse["companies"] as JArray;
    }

    private static bool ContainsOnlyDigits(string str)
    {
      return str.All(char.IsDigit);
    }
  }
}