using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using Newtonsoft.Json.Linq;
using ProffCompanyLookupService.Models;
using System.Collections.Generic;

namespace ProffCompanyLookupService
{
  public static class ProffCompanySearch
  {
    private static readonly HttpClient httpClient = new HttpClient();

    [FunctionName("ProffCompanySearch")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {

      string query = req.Query["query"];

      if (string.IsNullOrEmpty(query))
      {
        return new BadRequestObjectResult("Please provide a query parameter.");
      }

      var extractedData = await GetCompanyDataAsync(query);

      return new OkObjectResult(extractedData);
    }

    /// @summary Calls the Proff API and returns a JArray of company data.
    /// @return A JArray of company data.</returns>
    private static async Task<JArray> CallProffApiAsync(string query)
    {
      string proffApiKey = "PmWrTlGZhtzEh0xAWQP8cvFBX";
      string proffApiUrl = "https://api.proff.no/api/companies/register/NO";

      proffApiUrl = ContainsOnlyDigits(query) ? $"{proffApiUrl}/{query}" : $"{proffApiUrl}?Query={query}";

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

    private static JArray CreateJArrayFromApiResponse(JObject apiResponse)
    {
      // If the response contains a "companyTypeName" property, it's a single company object
      // Otherwise, return the "companies" array from the response
      return apiResponse.ContainsKey("companyTypeName") ? new JArray(apiResponse) : apiResponse["companies"] as JArray;
    }


    static bool ContainsOnlyDigits(string str)
    {
      return str.All(char.IsDigit);
    }

    /// @summary Processes the company data from the Proff API and returns a list of CompanyData objects.
    /// @return A list of CompanyData objects.</returns>
    private static async Task<List<CompanyData>> GetCompanyDataAsync(string query)
    {
      JArray companies = await CallProffApiAsync(query);

      return companies.Select(company =>
      {
        JObject phoneNumbers = company["phoneNumbers"] as JObject;
        JObject postalAddress = company["postalAddress"] as JObject;

        return new CompanyData
        {
          Name = company["name"]?.ToString(),
          OrganisationNumber = company["organisationNumber"]?.ToString(),
          Email = company["email"]?.ToString(),
          HomePage = company["homePage"]?.ToString(),
          MobilePhone = phoneNumbers?["mobilePhone"]?.ToString(),
          TelephoneNumber = phoneNumbers?["telephoneNumber"]?.ToString(),
          AddressLine = postalAddress?["addressLine"]?.ToString(),
          BoxAddressLine = postalAddress?["boxAddressLine"]?.ToString(),
          PostPlace = postalAddress?["postPlace"]?.ToString(),
          ZipCode = postalAddress?["zipCode"]?.ToString()
        };

      }).ToList();
    }
  }
}