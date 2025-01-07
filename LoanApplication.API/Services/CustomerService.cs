using LoanApplication.Models;
using LoanApplication.Repositories;
using LoanApplication.API.Configurations;
using Microsoft.Extensions.Options;
using LoanApplication.Models.Enums;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LoanApplication.API.Services;

public class CustomerService : ICustomerService
{
    private const int MONTHS_IN_YEAR = 12;
    private const double INTEREST_RATE_DIVISOR = 100;

    private readonly ICustomerRequestRepository _customerRequestRepository;
    private readonly BlacklistConfig _blacklistConfig;
    private readonly IConfiguration _configuration;

    public CustomerService(ICustomerRequestRepository customerRequestRepository, 
        IOptions<BlacklistConfig> blacklistConfig,
        IConfiguration configuration)
    {
        _customerRequestRepository = customerRequestRepository;
        _blacklistConfig = blacklistConfig.Value;
        _configuration = configuration;
    }

    public async Task<int> SaveCustomerRequestAsync(CustomerRequest request)
    {
        var existingCustomer = await GetCustomerByDetailsAsync(request.FirstName, request.LastName, request.DateOfBirth);
        if (existingCustomer == null)
        {
            return await _customerRequestRepository.AddAsync(request);
        }
        
        return existingCustomer.Id;
    }

    public async Task<CustomerRequest> GetCustomerByDetailsAsync(string firstName, string lastName, DateTime dateOfBirth)
    {
        return await _customerRequestRepository.GetByDetailsAsync(firstName, lastName, dateOfBirth);
    }

    public async Task<CustomerRequest> GetCustomerByIdAsync(int id)
    {
        return await _customerRequestRepository.GetByIdAsync(id);
    }

    public async Task<double> CalculateMonthlyRepayment(double principal, double annualRate, int months, Product product)
    {
        double monthlyInterestRate = (annualRate / INTEREST_RATE_DIVISOR) / MONTHS_IN_YEAR;
        double establishmentFee = _configuration.GetValue<double>("EstablishmentFee");
        double adjustedPrincipal = principal + establishmentFee;

        switch (product)
        {
            case Product.ProductA:
                return adjustedPrincipal / months;
            case Product.ProductB:
                if (months >= 6)
                {
                    double interestFreeRepayment = adjustedPrincipal / months;
                    double remainingPrincipal = adjustedPrincipal - (interestFreeRepayment * 2);
                    double remainingRepayment = CalculatePMT(monthlyInterestRate, (months-2), remainingPrincipal);

                    return (interestFreeRepayment * 2 + remainingRepayment * (months - 2)) / months;
                }
                else
                {
                    throw new Exception($"Duration must be at least 6 months for product {Product.ProductB}");
                }
            case Product.ProductC:
                return CalculatePMT(monthlyInterestRate, months, adjustedPrincipal);
            default:
                throw new ArgumentException("Invalid product selected");
        }
    }
    private static double CalculatePMT(double monthlyInterestRate, int totalPayments, double loanAmount)
    {
        return (monthlyInterestRate * loanAmount) / (1 - Math.Pow(1 + monthlyInterestRate, -totalPayments));
    }

    public async Task<(bool IsValid, string ValidationMessage)> ValidateApplicant(CustomerRequest request)
    {
        var today = DateTime.Today;
        var age = today.Year - request.DateOfBirth.Year;
        if (request.DateOfBirth.Date > today.AddYears(-age)) age--;

        if (age < 18)
        {
            return (false, "Applicant must be at least 18 years old.");
        }

        if (_blacklistConfig.MobileNumbers.Contains(request.Mobile))
        {
            return (false, "Mobile number is blacklisted.");
        }

        var emailDomain = request.Email.Split('@').Last();
        if (_blacklistConfig.EmailDomains.Contains(emailDomain))
        {
            return (false, "Email domain is blacklisted.");
        }

        return (true, string.Empty);
    }

    public async Task<int> UpdateCustomerDetailsAsync(CustomerRequest request)
    {
        return await _customerRequestRepository.UpdateAsync(request);
    }
}
