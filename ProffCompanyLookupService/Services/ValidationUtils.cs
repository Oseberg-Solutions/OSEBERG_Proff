using System.Linq;

namespace ProffCompanyLookupService.Services
{
  public static class ValidationUtils
  {
    public static (bool IsValid, string FormattedValue, string validationMessage) ValidateAndFormatOrgNr(string orgNr)
    {
      string validationMessage = "organisationNumber cannot be null or empty";
      if (string.IsNullOrEmpty(orgNr)) return (false, orgNr, validationMessage);

      string formattedOrgNr = orgNr.Replace(" ", "");

      bool isValid = formattedOrgNr.Length == 9 && formattedOrgNr.All(char.IsDigit);
      if (!isValid) validationMessage = "organisationNumber is not correct format";

      return (isValid, formattedOrgNr, validationMessage);
    }
  }
}