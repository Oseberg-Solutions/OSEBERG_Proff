using System.Net;
using Proff.Infrastructure;
using Proff.Services;
using Proff.ExternalServices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Proff.Models;
using LIVE_ProffCompanyLookup.Utils;

namespace Proff.Function
{
  public class ProffCompanyLookup
  {
    private const string AzureRequestTableActivityName = "ProffRequestActivity";
    private const string AzureConfigurationTableName = "ProffConfiguration";
    private const string HttpMessageMissingRequiredParameters = "Missing required parameters";
    private const string HttpMessageNoActiveSubscription = "No active subscription found";
    private readonly ILogger<ProffCompanyLookup> _logger;
    private static HttpResponseData? _response;
    private AzureTableStorageService _azureRequestActivityService;
    private AzureTableStorageService _azureConfigurationService;
    private ProffActivityService _proffActivityService;
    private ProffApiService _proffApiService;

    public ProffCompanyLookup(ILogger<ProffCompanyLookup> logger)
    {
      _logger = logger;
      _azureRequestActivityService = new AzureTableStorageService(AzureRequestTableActivityName);
      _azureConfigurationService = new AzureTableStorageService(AzureConfigurationTableName);
      _proffActivityService = new ProffActivityService(_azureRequestActivityService);
      _proffApiService = new ProffApiService(_azureConfigurationService);
    }

    [Function("ProffCompanyLookup")]
    public async Task<HttpResponseData> Run(
      [HttpTrigger(AuthorizationLevel.Function, "get")]
      HttpRequestData req, FunctionContext executionContext)
    {
      InputParams inputParams = new(req);

      if (!await _azureConfigurationService.EntityHasActiveSubscription(inputParams.domain))
      {
        return await HttpHelper.ConstructHttpResponse(_response, req, HttpStatusCode.BadRequest,
          HttpMessageNoActiveSubscription);
      }

      if (string.IsNullOrEmpty(inputParams.organisationNumber))
      {
        if (string.IsNullOrEmpty(inputParams.query) || string.IsNullOrEmpty(inputParams.country))
        {
          return await HttpHelper.ConstructHttpResponse(_response, req, HttpStatusCode.BadRequest,
            HttpMessageMissingRequiredParameters);
        }
        // Todo: Setup a cache for Proff Activty. Do we want 2 tables? One for detailed and one for 
        /*
        Vi må sette opp en cache mot Proff Activity. Lage ny tabell, tømme den 1 dagen hver 3 mnd.

        Vi må også fikse Proff premium til å gjøre spørring mot Azure som da henter Cache data.
        Vi må også fikse Gauage for de 3 likviditets greiene til å hente data fra Dataverse feltene, hvis det er.
        Hvis ikke det er data, så må de gjøre et søk først evt.

        ******************************************************************************************************
        Første tankene er vell at vi må lage cache for activity først. Litt usikker på hvordan vi gjør det 
        med tanke på Get Company og Get Detailed company info, siden det er nesten samme data så kan vi 
        bare ha en tabell og overskrive dataene som kommer fra detailed. Når vi henter data fra cached tabellen
        skal vi assume at detailed data er allrede hentet da?.

        1. lage en ProffActivityCache tabell, med kolonner man får fra Get COmpany Info og Detailed Comapny info.
        2. Lage logikk for å sette når vi gør spørringer mot APIET og hente disse dataene fra cache.
        3. Vi må også lage en logikk slik at vi tømmer tabellen hver 3 mnd. Evt når koden kjøres hver gang så sjekker den når første rad ble lagt til om det var for 3mnd siden så tømmer vi tabellen evt?.
        4. Lage logikk fra ProffPremium slik at Rating kan hente dataene sine fra Cache.

        5. Vi kan ikke ha cache for det første søket.
        */


        _logger.LogInformation("Get CompanyData...");

        var companies = await GetCompanyData(inputParams.query, inputParams.country);
        await _proffActivityService.UpdateRequestCountAsync(inputParams.domain);
        return await HttpHelper.ConstructHttpResponse(_response, req, HttpStatusCode.OK, companies);
      }

      _logger.LogInformation("Get Detailed Company Info");

      JObject extraCompanyInfo =
        await _proffApiService.GetDetailedCompanyInfoCopy(inputParams.country, inputParams.organisationNumber);
      await _proffActivityService.UpdateRequestCountAsync(inputParams.domain);
      return await HttpHelper.ConstructHttpResponse(_response, req, HttpStatusCode.OK, extraCompanyInfo);
    }

    private async Task<List<CompanyData>> GetCompanyData(string query, string country)
    {
      JArray companies = await _proffApiService.FetchCompanyDataAsync(query, country);
      CompanyDataService companyDataService = new();
      var companyDataList = companyDataService.ConvertJArrayToCompanyDataList(companies);
      return companyDataList;
    }
  }
}