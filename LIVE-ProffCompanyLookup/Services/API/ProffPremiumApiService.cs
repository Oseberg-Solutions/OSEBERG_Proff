﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Proff.Models;

namespace Proff.ExternalServices
{
  public class ProffPremiumApiService
  {
    private static HttpClient _httpClient;
    private readonly string _base_url = "https://ppc.proff.no";
    private readonly string _ProffPremiumApiToken;
    private readonly string PROFF_PREMIUM_API_TOKEN = "PROFF_PREMIUM_API_TOKEN";
    private const bool _isDebug = false;

    public ProffPremiumApiService()
    {
      _ProffPremiumApiToken = Environment.GetEnvironmentVariable(PROFF_PREMIUM_API_TOKEN);
      _httpClient = new HttpClient();
    }

    public async Task<(CreditRating, HttpStatusCode)> GetCreditScore(string organisationNumber)
    {
      if (_isDebug)
      {
        HttpStatusCode _statusCode = HttpStatusCode.OK;
        CreditRating testRating = new()
        {
          Economy = 1,
          LeadOwnership = 2,
          OrganisationNumber = 123456789,
          Rating = "C++",
          RatingScore = 34
        };

        return (testRating, _statusCode);
      }

      _httpClient.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _ProffPremiumApiToken);

      try
      {
        using var response = await _httpClient.GetAsync($"{_base_url}/CreditRating/{organisationNumber}");
        if (response.IsSuccessStatusCode)
        {
          var responseContent = await response.Content.ReadAsStringAsync();
          var creditRating = JsonConvert.DeserializeObject<CreditRating>(responseContent);

          return (creditRating, response.StatusCode);
        }
        else
        {
          return (null, response.StatusCode);
        }
      }
      catch (HttpRequestException)
      {
        return (null, HttpStatusCode.ServiceUnavailable);
      }
      catch (Exception)
      {
        return (null, HttpStatusCode.InternalServerError);
      }
    }
  }
}