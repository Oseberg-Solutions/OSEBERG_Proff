using Microsoft.Azure.Functions.Worker.Http;

namespace Proff.Models;

public class ParamPayload
{
  private string? _domain;
  private string? _query;
  private string? _country;
  private string? _proffCompanyId;

  public ParamPayload(HttpRequestData req)
  {
    _domain = string.IsNullOrEmpty(req.Query["domain"]) ? "Unknown" : req.Query["domain"];
    _query = req.Query["query"];
    _country = req.Query["country"];
    _proffCompanyId = req.Query["proffCompanyId"];
  }
}