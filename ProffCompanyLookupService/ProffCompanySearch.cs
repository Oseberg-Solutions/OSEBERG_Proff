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
        private static string PROFF_BASE_URL = "https://api.proff.no/api";

        [FunctionName("ProffCompanySearch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {

            // We want to be able to read multiple paramters?
            string query = req.Query["query"];
            string country = req.Query["country"];

            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(country))
            {
                return new BadRequestObjectResult("Missing required parameters");
            }


            JArray companies = await FetchCompanyDataFromProffApiAsync(query, country);

            var extractedData = ConvertJArrayToCompanyDataListAsync(companies);

            return new OkObjectResult(extractedData);
        }

        /// @summary Calls the Proff API and returns a JArray of company data.
        /// @return A JArray of company data.</returns>
        private static async Task<JArray> FetchCompanyDataFromProffApiAsync(string query, string country)
        {
            string proffApiKey = "PmWrTlGZhtzEh0xAWQP8cvFBX";

            string proffApiUrl = ContainsOnlyDigits(query)
                      ?
                      $"{PROFF_BASE_URL}/companies/register/{country}/{query}"
                      :
                      $"{PROFF_BASE_URL}/companies/eniropro/{country}?name={query}";

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
        private static List<CompanyData> ConvertJArrayToCompanyDataListAsync(JArray companies)
        {
            return companies.Select(company =>
            {
                JObject phoneNumbers = company["phoneNumbers"] as JObject;
                JObject postalAddress = company["postalAddress"] as JObject;

                return new CompanyData
                {
                    Name = company["name"]?.ToString(),
                    CompanyTypeName = company["companyTypeName"]?.ToString(),
                    NumberOfEmployees = company["numberOfEmployees"]?.ToString(),
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