using LoanApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace LoanApplication.UI.Pages;

public class QuoteResultModel : PageModel
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public QuoteResultModel(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    [BindProperty]
    public CustomerRequest CustomerRequest { get; set; }
    public bool IsPersonalInfoUpdated { get; set; } = false;
    public bool IsFinancialInfoUpdated { get; set; } = false;
    public async Task OnGetAsync()
    {
        ViewData["ApiUrl"] = _configuration["ApiSettings:BaseUrl"];
        var sessionData = HttpContext.Session.GetString("CustomerRequest");

        if (sessionData != null)
        {
            CustomerRequest = JsonConvert.DeserializeObject<CustomerRequest>(sessionData);
            
            //checl if need to reload the model
            if(IsFinancialInfoUpdated || IsPersonalInfoUpdated)
            {
                await LoadCustomerRequestInfo();
            }
        }
    }

    private async Task LoadCustomerRequestInfo()
    {
        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");
        var apiUrl = $"{baseUrl}Customer/{CustomerRequest.Id}";
        var response = await _httpClient.GetAsync(apiUrl);
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var customerRequest = JsonConvert.DeserializeObject<CustomerRequest>(responseContent);

                if (customerRequest != null)
                {
                    CustomerRequest = customerRequest;
                }
            }
            catch (JsonException ex)
            {
                var rawJson = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, "Invalid JSON format");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Unexpected error: {ex.Message}");
            }

        }
        else
        {
            ModelState.AddModelError(string.Empty, "Customer not found.");
        }
    }

    private class RepaymentDetails
    {
        public double MonthlyRepayment { get; set; }
        public double TotalRepayment { get; set; }
        public double InterestAmount { get; set; }
        public LoanApplication.Models.Enums.Product Product { get; set; }

    }
    public async Task<IActionResult> OnPostAsync(string updateType)
    {
        switch (updateType)
        {
            case "personalInfo":
                IsPersonalInfoUpdated = true;
                break;
            case "financeInfo":
                IsFinancialInfoUpdated = true;
                break;
        }

        await OnGetAsync();  // Re-fetch the data

        return Page();
    }

}
