using Microsoft.Azure.Functions.Worker.Http;

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
    organisationNumber = req.Query["organisationNumber"];
  }
}