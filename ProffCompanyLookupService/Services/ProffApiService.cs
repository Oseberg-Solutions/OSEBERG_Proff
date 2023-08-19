using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static System.Net.WebRequestMethods;

public class ProffApiService
{
  private static readonly HttpClient httpClient = new HttpClient();
  private static string PROFF_BASE_URL = "https://api.proff.no/api";
  private string proffApiKey = "PmWrTlGZhtzEh0xAWQP8cvFBX";

  public async Task<JArray> FetchCompanyDataAsync(string query, string country)
  {
    string proffApiUrl = ContainsOnlyDigits(query)
        ? $"{PROFF_BASE_URL}/companies/eniropro/{country}?industry={query}"
        : $"{PROFF_BASE_URL}/companies/eniropro/{country}?name={query}";

    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", proffApiKey);
    HttpResponseMessage response = await httpClient.GetAsync(proffApiUrl);
    string responseContent = await response.Content.ReadAsStringAsync();

    SaveApiResponseToFile(responseContent, @"C:\git\PCF-Component\ProffCompanyLookupService\FetchCompanyData.json");


    if (!response.IsSuccessStatusCode)
    {
      throw new Exception($"Error calling Proff API: {responseContent}");
    }

    JObject apiResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
    return CreateJArrayFromApiResponse(apiResponse);
  }

  public async Task<(string NumberOfEmployees, string Nace)> GetDetailedCompanyInfo(string country, string proffCompanyId)
  {
    string proffCompanyListingUrl = $"{PROFF_BASE_URL}/companies/eniropro/{country}/{proffCompanyId}";

    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", proffApiKey);
    HttpResponseMessage response = await httpClient.GetAsync(proffCompanyListingUrl);

    if (!response.IsSuccessStatusCode)
    {
      string responseContent = await response.Content.ReadAsStringAsync();
      throw new Exception($"Error calling Proff API for detailed info: {responseContent}");
    }

    // Capture the full API response
    string apiResponseContent = await response.Content.ReadAsStringAsync();

    SaveApiResponseToFile(apiResponseContent, @"C:\git\PCF-Component\ProffCompanyLookupService\proffApiResponse.json");

    JObject apiResponse = JObject.Parse(apiResponseContent);

    // Extract the NumberOfEmployees and Nace data from the API response
    string numberOfEmployees = apiResponse["registerListing"]["numberOfEmployees"]?.ToString();
    string nace = apiResponse["registerListing"]["naceCategories"]?[0]?.ToString();


    return (numberOfEmployees, nace);
  }

  private void SaveApiResponseToFile(string content, string filePath)
  {
    System.IO.File.WriteAllText(filePath, content);
  }



  private JArray CreateJArrayFromApiResponse(JObject apiResponse)
  {
    return apiResponse.ContainsKey("companyTypeName") ? new JArray(apiResponse) : apiResponse["companies"] as JArray;
  }

  private bool ContainsOnlyDigits(string str)
  {
    return str.All(char.IsDigit);
  }
}