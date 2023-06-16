using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class ProffApiService
{
  private static readonly HttpClient httpClient = new HttpClient();
  private static string PROFF_BASE_URL = "https://api.proff.no/api";
  private string proffApiKey = "PmWrTlGZhtzEh0xAWQP8cvFBX";

  public async Task<JArray> FetchCompanyDataAsync(string query, string country)
  {
    string proffApiUrl = ContainsOnlyDigits(query)
        ? $"{PROFF_BASE_URL}/companies/register/{country}/{query}"
        : $"{PROFF_BASE_URL}/companies/eniropro/{country}?name={query}";

    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", proffApiKey);
    HttpResponseMessage response = await httpClient.GetAsync(proffApiUrl);

    if (!response.IsSuccessStatusCode)
    {
      string responseContent = await response.Content.ReadAsStringAsync();
      throw new Exception($"Error calling Proff API: {responseContent}");
    }

    JObject apiResponse = JObject.Parse(await response.Content.ReadAsStringAsync());

    return CreateJArrayFromApiResponse(apiResponse);
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
