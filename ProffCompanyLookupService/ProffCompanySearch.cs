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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ProffCompanyLookupService
{
  public static class ProffCompanySearch
  {
    private static readonly HttpClient httpClient = new HttpClient();
    private static string PROFF_BASE_URL = "https://api.proff.no/api";
    private static string storageAccountTableName = "ProffDomains";
    private static readonly string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=proffwebstorage;AccountKey=LOMG727/G2kA7AbOFIMfEg7aiF+xYiDGVcL44yUb92F3FcZ2vpdEp5AprrTck6UA9VxU8NVR8GFv+ASto5GNzQ==;EndpointSuffix=core.windows.net";


    [FunctionName("ProffCompanySearch")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
      try
      {

        string domain = req.Query["domain"];
        log.LogInformation($"Domain: {domain}");
        // Retrieve the requesting domain from the Forwarded header
        LogRequestHeaders(req, log);

        /*
              string domain = req.Query["domain"];
              string query = req.Query["query"];
              string country = req.Query["country"];

              if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(country))
              {
                return new BadRequestObjectResult("Missing required parameters");
              }

              JArray companies = await FetchCompanyDataFromProffApiAsync(query, country);
              var extractedData = ConvertJArrayToCompanyDataListAsync(companies);

              return new OkObjectResult(extractedData);
        */
        /*
            TODO: We must check if requestingDomain is legit, and maybe that it creates a new row 
            in our azure Table. so we can easily find it and add amount_of_requests.
        */

        // log.LogInformation($"Requesting domain: {requestingDomain}");

        await UpdateProffDomainsTable();

        return new OkObjectResult("Smosh Smosh");
      }
      catch (Exception ex)
      {
        log.LogError(ex, "Error occurred during function execution.");
        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
      }
    }

    private static void LogRequestHeaders(HttpRequest req, ILogger log)
    {
      string requestingDomain = req.Headers["Referer"].ToString();
      foreach (var header in req.Headers)
      {
        Console.WriteLine($"{header.Key}: {header.Value}");
        log.LogInformation($"{header.Key}: {header.Value}");
      }
      Console.WriteLine($"Requesting domain: {requestingDomain}");
      log.LogInformation($"Requesting domain: {requestingDomain}");
    }

    private static async Task UpdateProffDomainsTable()
    {
      // Get a reference to the Azure Table storage
      CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
      CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
      CloudTable table = tableClient.GetTableReference(storageAccountTableName);

      // Define the query to fetch rows based on the desired criteria
      string rowKey = "localhost";
      TableQuery<DynamicTableEntity> tableQuery = new TableQuery<DynamicTableEntity>()
          .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey))
          .Take(1); // Fetch only one row

      // Execute the query and retrieve the matching entity
      TableQuerySegment<DynamicTableEntity> segment = await table.ExecuteQuerySegmentedAsync(tableQuery, null);
      DynamicTableEntity entity = segment.FirstOrDefault();

      if (entity != null)
      {
        Console.WriteLine($"PartitionKey: {entity.PartitionKey}");
        Console.WriteLine($"RowKey: {entity.RowKey}");

        var amountOfRequests = entity.Properties["amount_of_request"].Int32Value ?? 0;
        amountOfRequests++;
        entity.Properties["amount_of_request"] = new EntityProperty(amountOfRequests);

        // Save the updated entity back to the table
        TableOperation updateOperation = TableOperation.Replace(entity);
        await table.ExecuteAsync(updateOperation);

        Console.WriteLine("Amountofreqeusts: " + amountOfRequests);
      }
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