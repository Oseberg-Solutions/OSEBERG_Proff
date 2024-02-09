using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProffCompanyLookupService.Services;
using System.Web.Http;
using ProffCompanyLookupService.Models;
using System.Net;

namespace ProffCompanyLookupService
{
  public static class ProffPremiumCredit
  {
    [FunctionName("ProffPremiumCredit")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
      string orgNr = req.Query["orgNr"];
      var (isValid, formattedOrgNr) = ValidationUtils.ValidateAndFormatOrgNr(orgNr);

      if (!isValid)
      {
        return new BadRequestObjectResult("Invalid organisation number format.");
      }

      ProffPremiumApiService proffPremiumApiService = new ProffPremiumApiService();
      var (creditRating, isSuccess, statusCode) = await proffPremiumApiService.GetCreditScore(formattedOrgNr);

      return HandleResponse(creditRating, isSuccess, statusCode);
    }

    private static IActionResult HandleResponse(CreditRating creditRating, bool isSuccess, HttpStatusCode statusCode)
    {
      switch (statusCode)
      {
        case HttpStatusCode.OK:
          return new OkObjectResult(creditRating);
        case HttpStatusCode.NoContent:
          return new NotFoundObjectResult("Company record not found.");
        case HttpStatusCode.InternalServerError:
          return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        default:
          return new StatusCodeResult((int)statusCode);
      }
    }
  }
}