using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProffCompanyLookupService.Models;

namespace ProffCompanyLookupService.Services
{
  public class ProffPremiumApiService
  {
    private static HttpClient httpClient;
    private string _ProffPremiumApiToken;
    private string _base_url = "https://ppc.proff.no";
    ProffPremiumApiService()
    {
      _ProffPremiumApiToken = Environment.GetEnvironmentVariable("PROFF_PREMIUM_API_TOKEN");
      httpClient = new HttpClient();
    }

    public async Task<CreditRating> GetCreditScore(string organisationNumber)
    {
      httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _ProffPremiumApiToken);
      HttpResponseMessage response = await httpClient.GetAsync($"{_base_url}/CreditRating/{organisationNumber}");


      if (response.StatusCode == System.Net.HttpStatusCode.OK)
      {
        string responseContent = await response.Content.ReadAsStringAsync();
        CreditRating creditRating = JsonConvert.DeserializeObject<CreditRating>(responseContent);
        return creditRating;
      }
      else if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
      {

        // Lets return a object that tells we didnt find records...
        return null;
      }
      else
      {
        // Something went wrong ?
        return null;
      }
    }
  }
}
