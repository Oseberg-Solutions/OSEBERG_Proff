using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Proff.Models;
using System.Net;

namespace LIVE_ProffCompanyLookup.Utils
{
  public static class HttpHelper
  {
    public static async Task<HttpResponseData> ConstructHttpResponse(HttpResponseData? _response, HttpRequestData req, HttpStatusCode statusCode,
 JObject extraCompanyInfo)
    {
      _response = req.CreateResponse(statusCode);
      string jsonString = JsonConvert.SerializeObject(extraCompanyInfo);
      _response.Headers.Add("Content-Type", "application/json; charset=utf-8");
      await _response.WriteStringAsync(jsonString);
      return _response;
    }

    public static async Task<HttpResponseData> ConstructHttpResponse(HttpResponseData? _response, HttpRequestData req, HttpStatusCode statusCode,
      string message)
    {
      _response = req.CreateResponse(statusCode);
      _response.Headers.Add("Content-Type", "application/json; charset=utf-8");
      await _response.WriteStringAsync(message);
      return _response;
    }

    public static async Task<HttpResponseData> ConstructHttpResponse(HttpResponseData? _response, HttpRequestData req, HttpStatusCode statusCode,
      List<CompanyData> companyData)
    {
      _response = req.CreateResponse(statusCode);
      string jsonString = JsonConvert.SerializeObject(companyData);
      _response.Headers.Add("Content-Type", "application/json; charset=utf-8");
      await _response.WriteStringAsync(jsonString);
      return _response;
    }
  }
}
