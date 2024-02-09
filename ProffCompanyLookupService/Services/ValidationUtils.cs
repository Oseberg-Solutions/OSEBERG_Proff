using System.Linq;

namespace ProffCompanyLookupService.Services
{
  public static class ValidationUtils
  {
    public static (bool IsValid, string FormattedValue) ValidateAndFormatOrgNr(string orgNr)
    {
      if (string.IsNullOrEmpty(orgNr)) return (false, orgNr);

      string formattedOrgNr = orgNr.Replace(" ", "");

      bool isValid = formattedOrgNr.Length == 11 && formattedOrgNr.All(char.IsDigit);

      return (isValid, formattedOrgNr);
    }
  }
}