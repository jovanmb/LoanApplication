using LoanApplication.Models;
using LoanApplication.Models.Enums;

namespace LoanApplication.API.Services;
public interface ICustomerService
{
    Task<int> SaveCustomerRequestAsync(CustomerRequest request);
    Task<CustomerRequest> GetCustomerByDetailsAsync(string firstName, string lastName, DateTime dateOfBirth);
    Task<double> CalculateMonthlyRepayment(double principal, double annualRate, int months, Product product);
    Task<(bool IsValid, string ValidationMessage)> ValidateApplicant(CustomerRequest request);
    Task<CustomerRequest>GetCustomerByIdAsync(int id);
    Task<int> UpdateCustomerDetailsAsync(CustomerRequest request);
}
