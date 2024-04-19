using System.Net;
using System.Text.Json;
using Azure.Data.Tables;
using LIVE_ProffCompanyLookup.Infrastructure;
using LIVE_ProffCompanyLookup.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Proff.Models;

namespace Proff.Function
{
  public class ProffCompanyLookup
  {
    private readonly ILogger<ProffCompanyLookup> _logger;
    private static readonly string _storageAccountTableName = "ProffRequestActivity";

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
      ParamPayload paramPayLoad = new(req);
      
      
      
      
      
      string domain = string.IsNullOrEmpty(req.Query["domain"]) ? "Unknown" : req.Query["domain"];

      var response = req.CreateResponse(HttpStatusCode.OK);

      var suran = new Company { Name = "Suran Corporation", Description = "A leading technology company." };
      
      var jsonString = JsonSerializer.Serialize(suran);

      AzureTableStorageService storageService = new(_storageAccountTableName);

      var activityService = new ProffActivityService(storageService);
      await activityService.UpdateRequestCountAsync("suran.localhost");


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