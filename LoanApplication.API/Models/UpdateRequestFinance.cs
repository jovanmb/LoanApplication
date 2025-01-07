using LoanApplication.Models.Enums;

namespace LoanApplication.API.Models;

public class UpdateRequestFinance
{
    public double AmountRequired { get; set; }
    public int Term { get; set; }
    public Product Product { get; set; }
}
