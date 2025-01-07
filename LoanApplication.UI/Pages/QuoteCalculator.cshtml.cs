using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LoanApplication.Models;
using Newtonsoft.Json;
using LoanApplication.Models.Enums;
using Newtonsoft.Json.Converters;

namespace LoanApplication.UI.Pages;

public class QuoteCalculatorModel : PageModel
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public QuoteCalculatorModel(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    [BindProperty]
    public CustomerRequest CustomerRequest { get; set; }

    public async Task<IActionResult> OnGetAsync(int? customerId)
    {
        if (customerId == null)
        {
            return NotFound();
        }

        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");
        var apiUrl = $"{baseUrl}Customer/{customerId.Value}";
        var response = await _httpClient.GetAsync(apiUrl);
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var customerRequest = JsonConvert.DeserializeObject<CustomerRequest>(responseContent);

                if (customerRequest == null)
                {
                    ModelState.AddModelError(string.Empty, "Failed to deserialize the customer request.");
                    // Handle the error appropriately, maybe log it for further investigation
                    return Page();
                }

                CustomerRequest = customerRequest;
            }
            catch (JsonException ex)
            {
                var rawJson = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, "Invalid JSON format");
                // Log the exception for further investigation
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Unexpected error: {ex.Message}");
                // Log the exception for further investigation
            }

        }
        else
        {
            ModelState.AddModelError(string.Empty, "Customer not found.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? customerId)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (customerId != null)
        {
            CustomerRequest.Id = customerId.Value;
        }

        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");
        var apiUrl = $"{baseUrl}Customer/calculate";
        var calculateResponse = await _httpClient.PostAsJsonAsync(apiUrl, CustomerRequest);

        if (calculateResponse.IsSuccessStatusCode)
        {
            var jsonString = await calculateResponse.Content.ReadAsStringAsync();
            Console.WriteLine("JSON Response: {0}", jsonString);

            try
            {
                var repaymentDetails = JsonConvert.DeserializeObject<RepaymentDetails>(jsonString);

                CustomerRequest.MonthlyRepayment = repaymentDetails.MonthlyRepayment;
                CustomerRequest.TotalRepayment = repaymentDetails.TotalRepayment;
                CustomerRequest.InterestAmount = repaymentDetails.InterestAmount;
                CustomerRequest.EstablishmentFee = _configuration.GetValue<double>("EstablishmentFee");
                CustomerRequest.Product = repaymentDetails.Product;

                // Update the customer request info
                var updateResult = await UpdateCustomerRequestInfoAsync(CustomerRequest);
                if (updateResult > 0)
                {
                    HttpContext.Session.SetString("CustomerRequest", JsonConvert.SerializeObject(CustomerRequest));
                    return RedirectToPage("QuoteResult");
                }
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine("Error deserializing JSON response: {0}", jsonString);
                ModelState.AddModelError(string.Empty, "An error occurred while processing the response from the server.");
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, "An error occurred while calculating the repayment.");
        }

        return Page();
    }


    private async Task<int> UpdateCustomerRequestInfoAsync(CustomerRequest request)
    {
        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");
        var apiUrl = $"{baseUrl}Customer/updateCustomerRequestInfo/{request.Id}";
        var updateResponse = await _httpClient.PutAsJsonAsync(apiUrl, request);

        if (updateResponse.IsSuccessStatusCode)
        {
            var result = await updateResponse.Content.ReadAsStringAsync();
            if (int.TryParse(result, out int updateResult))
            {
                return updateResult;
            }
        }
        return 0;
    }

    private class RepaymentDetails
    {
        [JsonProperty("monthlyRepayment")]
        public double MonthlyRepayment { get; set; }

        [JsonProperty("totalRepayment")]
        public double TotalRepayment { get; set; }

        [JsonProperty("interestAmount")]
        public double InterestAmount { get; set; }

        [JsonProperty("product")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Product Product { get; set; }
    }

}
