using Microsoft.Azure.Functions.Worker.Http;

namespace Proff.Models;

public class ParamPayload
{
  public string? domain;
  public string? query;
  public string? country;
  public string? proffCompanyId;

  public ParamPayload(HttpRequestData req)
  {
    domain = string.IsNullOrEmpty(req.Query["domain"]) ? "Unknown" : req.Query["domain"];
    query = req.Query["query"];
    country = req.Query["country"];
    proffCompanyId = req.Query["proffCompanyId"];
  }
}