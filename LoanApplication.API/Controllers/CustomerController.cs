
using LoanApplication.API.Models;
using LoanApplication.API.Services;
using LoanApplication.Models;
using LoanApplication.Models.Enums;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplicationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IConfiguration _configuration;

    public CustomerController(ICustomerService customerService,
        IConfiguration configuration)
    {
        _customerService = customerService;
        _configuration = configuration;
    }

    [HttpPost("save")]
    public async Task<IActionResult> CreateCustomer([FromBody] CustomerRequest request)
    {
        if(request == null)
        {
            return BadRequest(
                new 
                {
                    ErrorResponse = "Empty request submitted"
                }
            );
        }

        var customerId = await _customerService.SaveCustomerRequestAsync(request);
        var baseUrl = _configuration.GetValue<string>("UISettings:BaseUrl");
        var redirectUrl = $"{baseUrl}/QuoteCalculator/Customer/{customerId}";
        
        return Ok(new { RedirectUrl = redirectUrl });
    }

    [HttpPost("apply")]
    public async Task<IActionResult> SubmitLoanApplication ([FromBody] CustomerRequest request)
    {
        var validationStatus = await _customerService.ValidateApplicant(request);

        if (!validationStatus.IsValid)
        {
            var errorResponse = new { Message = validationStatus.ValidationMessage };
            return new ObjectResult(errorResponse) { StatusCode = 400 };
        }

        var submitResult = await _customerService.UpdateCustomerDetailsAsync(request);
        if(submitResult <= 0)
        {
            return BadRequest(new { Message = "Unable to save customer request record" });
        }

        return Ok(submitResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(int id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
        {
            return NotFound("Customer request not found.");
        }
        return Ok(customer);
    }
    
    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateRepayment([FromBody] CustomerRequest request)
    {
        if (request == null)
        {
            return BadRequest("Invalid request");
        }

        var repaymentDetails = await CalculateMonthlyRepaymentAsync(request);
        var repaymentInfo= new 
        { 
            MonthlyRepayment = repaymentDetails.monthlyRepayment, 
            TotalRepayment = repaymentDetails.totalRepayment, 
            InterestAmount = repaymentDetails.interestAmount,
            Product = repaymentDetails.product,
        };

        return Ok(repaymentInfo);
    }

    [HttpPut("updatePersonalInfo/{id}")]
    public async Task<IActionResult> UpdatePersonalInfo(int id, [FromBody] UpdateRequestInfo requestInfo)
    {
        var customerRequest = await _customerService.GetCustomerByIdAsync(id);
        if(customerRequest == null)
        {
            return NotFound("Customer does not exist");
        }

        customerRequest.FirstName = requestInfo.FirstName;
        customerRequest.LastName = requestInfo.LastName;
        customerRequest.Email = requestInfo.Email;
        customerRequest.Mobile = requestInfo.Mobile;

        await _customerService.UpdateCustomerDetailsAsync(customerRequest);
        
        return Ok(customerRequest);
    }

    [HttpPut("updateFinancialInfo/{id}")]
    public async Task<IActionResult> UpdateFinancialInfo(int id, [FromBody] UpdateRequestFinance requestFinanceInfo)
    {
        var customerRequest = await _customerService.GetCustomerByIdAsync(id);
        if (customerRequest == null)
        {
            return NotFound("Customer does not exist");
        }

        customerRequest.Product= requestFinanceInfo.Product;
        customerRequest.AmountRequired = requestFinanceInfo.AmountRequired;
        customerRequest.Term = requestFinanceInfo.Term;

        //re-calculate monthly payment
        var repaymentDetails = await CalculateMonthlyRepaymentAsync(customerRequest);
        customerRequest.MonthlyRepayment = repaymentDetails.monthlyRepayment;
        customerRequest.TotalRepayment = repaymentDetails.totalRepayment;
        customerRequest.InterestAmount = repaymentDetails.interestAmount;

        var updateResult = await _customerService.UpdateCustomerDetailsAsync(customerRequest);
        if (updateResult <= 0)
        {
            return BadRequest("Cannot update customer info");
        }

        return Ok(customerRequest);
    }

    [HttpPut("updateCustomerRequestInfo/{id}")]
    public async Task<IActionResult> UpdateCustomerRequestInfo(int id, [FromBody] CustomerRequest request)
    {
        var customerRequest = await _customerService.GetCustomerByIdAsync(id);
        if (customerRequest == null)
        {
            return NotFound("Customer does not exist");
        }

        customerRequest = request;

        var updateResult = await _customerService.UpdateCustomerDetailsAsync(customerRequest);
        return Ok(updateResult);
    }

    private async Task<(double monthlyRepayment, double totalRepayment, double interestAmount, Product product)> CalculateMonthlyRepaymentAsync(CustomerRequest request)
    {
        var annualRate = _configuration.GetValue<double>("AnnualRate");

        double monthlyRepayment = await _customerService.CalculateMonthlyRepayment(request.AmountRequired, annualRate, request.Term, request.Product);
        double totalRepayment = monthlyRepayment * request.Term;
        double interestAmount = (request.Product == LoanApplication.Models.Enums.Product.ProductA) ? 0 : totalRepayment - request.AmountRequired;
        Product product = request.Product;

        return (monthlyRepayment, totalRepayment, interestAmount, product);
    }

}
