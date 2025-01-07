using LoanApplication.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LoanApplication.Models;
public class CustomerRequest
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    public double AmountRequired { get; set; }
    public int Term { get; set; }

    [JsonConverter(typeof(StringEnumConverter))] 
    public Title Title { get; set; }

    [JsonConverter(typeof(StringEnumConverter))] 
    public Product Product { get; set; }
    public double? MonthlyRepayment { get; set; }
    public double? TotalRepayment { get; set; }
    public double? InterestAmount { get; set; }
    public double? EstablishmentFee { get; set; }
}

