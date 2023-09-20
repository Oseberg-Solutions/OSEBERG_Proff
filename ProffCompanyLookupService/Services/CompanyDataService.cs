using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using ProffCompanyLookupService.Models;

public class CompanyDataService
{
  public List<CompanyData> ConvertJArrayToCompanyDataList(JArray companies)
  {
    return companies.Select(company =>
    {
      JObject phoneNumbers = company["phoneNumbers"] as JObject;
      JObject postalAddress = company["postalAddress"] as JObject;
      JObject visitorAddress = company["visitorAddress"] as JObject;

      return new CompanyData
      {
        Name = company["name"]?.ToString(),
        ProffCompanyId = company["companyId"]?.ToString(),
        CompanyTypeName = company["companyTypeName"]?.ToString(),
        NumberOfEmployees = company["numberOfEmployees"]?.ToString(),
        OrganisationNumber = company["organisationNumber"]?.ToString(),
        Email = company["email"]?.ToString(),
        HomePage = company["homePage"]?.ToString(),
        MobilePhone = phoneNumbers?["mobilePhone"]?.ToString(),
        TelephoneNumber = phoneNumbers?["telephoneNumber"]?.ToString(),
        AddressLine = postalAddress?["addressLine"]?.ToString(),
        BoxAddressLine = postalAddress?["boxAddressLine"]?.ToString(),
        PostPlace = postalAddress?["postPlace"]?.ToString(),
        ZipCode = postalAddress?["zipCode"]?.ToString(),

        VisitorAddressLine = visitorAddress?["addressLine"]?.ToString(),
        VisitorBoxAddressLine = visitorAddress?["boxAddressLine"]?.ToString(),
        VisitorPostPlace = visitorAddress?["postPlace"]?.ToString(),
        VisitorZipCode = visitorAddress?["zipCode"]?.ToString(),
      };
    }).ToList();
  }
}
