using System;

namespace ProffCompanyLookupService.Models
{
  public class CreditRating
  {
    public string Rating { get; set; }
    public DateTime Updated { get; set; }
    public int Economy { get; set; }
    public int LeadOwnership { get; set; }
    public int PaymentHistory { get; set; }
    public int OtherGeneral { get; set; }
    public int OrganisationNumber { get; set; }
    public int RatingScore { get; set; }
  }
}