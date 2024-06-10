using Microsoft.Azure.Functions.Worker.Http;
using System.Text.RegularExpressions;

namespace Proff.Models;

public class InputParams
{
  public string? domain;
  public string? query;
  public string? country;
  public string? organisationNumber;

  public InputParams(HttpRequestData req)
  {
    domain = string.IsNullOrEmpty(req.Query["domain"]) ? "Unknown" : req.Query["domain"];
    query = req.Query["query"];
    country = req.Query["country"];
    organisationNumber = CleanOrganisationNumber(req.Query["organisationNumber"]);
  }
  private string? CleanOrganisationNumber(string? orgNumber)
  {
    if (orgNumber != null)
    {
      string cleanedNumber = Regex.Replace(orgNumber, @"\s", "");

      cleanedNumber = Regex.Replace(cleanedNumber, @"\D", "");

      return cleanedNumber;
    }

    return null;
  }
}