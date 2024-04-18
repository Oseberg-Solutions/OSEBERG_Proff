using System.Net;
using System.Text.Json;
using Azure.Data.Tables;
using LIVE_ProffCompanyLookup.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Proff.Function
{
  public class ProffCompanyLookup
  {
    private readonly ILogger<ProffCompanyLookup> _logger;
    private readonly static string _storageAccountTableName = "ProffRequestActivity";

    public ProffCompanyLookup(ILogger<ProffCompanyLookup> logger)
    {
      _logger = logger;
    }

    [Function("ProffCompanyLookup")]
    public async Task<HttpResponseData> Run(
      [HttpTrigger(AuthorizationLevel.Function, "get", "post")]
      HttpRequestData req)
    {
      string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

      var response = req.CreateResponse(HttpStatusCode.OK);

      var suran = new Company { Name = "Suran Corporation", Description = "A leading technology company." };
      var jsonString = JsonSerializer.Serialize(suran);

      AzureTableStorageService storageService = new(_storageAccountTableName);

      
      await response.WriteStringAsync(jsonString);
      return response;
    }
  }

  public class Company
  {
    public string Name { get; set; }
    public string Description { get; set; }
  }
}