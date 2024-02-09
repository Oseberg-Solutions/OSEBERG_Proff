using System;
using System.Net;
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
    public ProffPremiumApiService()
    {
      _ProffPremiumApiToken = Environment.GetEnvironmentVariable("PROFF_PREMIUM_API_TOKEN");
      httpClient = new HttpClient();
    }

    public async Task<(CreditRating, bool, HttpStatusCode)> GetCreditScore(string organisationNumber)
    {
      httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _ProffPremiumApiToken);

      try
      {
        using (var response = await httpClient.GetAsync($"{_base_url}/CreditRating/{organisationNumber}"))
        {
          if (response.IsSuccessStatusCode)
          {
            var responseContent = await response.Content.ReadAsStringAsync();
            var creditRating = JsonConvert.DeserializeObject<CreditRating>(responseContent);
            return (creditRating, true, response.StatusCode);
          }
          else
          {
            return (null, false, response.StatusCode);
          }
        }
      }
      catch (HttpRequestException)
      {
        return (null, false, HttpStatusCode.ServiceUnavailable);
      }
      catch (Exception)
      {
        return (null, false, HttpStatusCode.InternalServerError);
      }
    }
  }
}
