using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProffCompanyLookupService.Models
{
  public class CompanyData
  {
    public string Name { get; set; }
    public string ProffCompanyId { get; set; }
    public string CompanyTypeName { get; set; }
    public string OrganisationNumber { get; set; }
    public string Email { get; set; }
    public string HomePage { get; set; }
    public string MobilePhone { get; set; }
    public string TelephoneNumber { get; set; }
    public string AddressLine { get; set; }
    public string BoxAddressLine { get; set; }
    public string PostPlace { get; set; }
    public string ZipCode { get; set; }
    public string NumberOfEmployees { get; set; }
    public string Nace { get; set; }
    public string Profit { get; set; }
    public string Revenue { get; set; }
    public string VisitorAddressLine { get; set; }
    public string VisitorBoxAddressLine { get; set; }
    public string VisitorPostPlace { get; set; }
    public string VisitorZipCode { get; set; }
    public string Likviditetsgrad { get; set; }
    public string TotalrentabilitetLoennsomhet { get; set; }
    public string Egenkapitalandel { get; set; }
  }
}
