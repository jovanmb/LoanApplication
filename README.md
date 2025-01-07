# LoanApplication

##### This repository contains the Loan Application UI built with ASP.NET Core Razor Pages. The application allows users to calculate loan quotes, update personal and financial information, and apply for loans. 

## Features 
- Calculate loan quotes based on user input
- Edit personal and financial information via modals
- Apply for a loan with updated details
- Server-side API integration for data retrieval and updates
- Responsive design with Bootstrap

## Prerequesites
- .Net Core 6 and Up
- Visual Studio 2022 / VS Code
- Bootstrap (included via CDN)
- NewtonSoft.Json

### Installation
1. Clone the repository.
2. Open/Navigate to the direcroty
3. Open the solution LoanApplication.sln
   - Right-click and click properties select Multiple startup projects option and select start for the following:
       - LoanApplication.API
       - LoanApplication.UI
4. Click "Apply" and "Ok".

### LoanApplicatio.API settings:
1. Install Microsoft.EntityFrameworkCore
2. Install Microsoft.EntityFrameworkCore.InMemory (we will use In memory database)
3. Appsettings.json (add the following properties):
    - "Blacklists": {
      "MobileNumbers": [ "0422111333", "0412345678" ],
      "EmailDomains": [ "blacklist.com", "spamdomain.com" ]
   }
    - "UISettings": {
      "BaseUrl": "https://localhost:7230"
   }
    - "LoanMinimumDuration": "6"
    - "EstablishmentFee": "300.00"
    - "AnnualRate":  "2.00"
4. Feel free to modify the values for PMT computation, port of the UI project and for validations)

### LoanApplication.UI
1. Set API port and url Appsettings.json properties:
   - "ApiSettings": {
        "BaseUrl": "https://localhost:7089/api/"
      }

### Running application
1. Open solution LoanApplication.Sln in Visual Studio
1. Right-click on LoanApplication.Sln (solution file)
   - Clean solution
   - Rebuild Solution
2. Press F5 or the play icon
3. Make sure that Swagger (Api) and Razor (UI) loads up.
4. Navigate to Swager (API site) :
   - Open the Save endpoint (/api/customer/save)
   - Click the try it out and paste this sample payload and hit execute:
     - {
        "firstName": "John",
        "lastName": "Doe",
        "dateOfBirth": "1990-01-01",
        "amountRequired": "5000",
        "term": "12",
        "title": "Mr",
        "email": "john.doe@example.com",
        "mobile": "0422111333",
        "product": "ProductA",
        "monthlyRepayment": null,
        "totalRepayment": null,
        "interestAmount": null,
        "establishmentFee": null
       }
     - copy the returned URL ex. (https://localhost:7230/QuoteCalculator/Customer/1)
5. Navigate to UI application browser and paste the url in the address bar
6. Quote Calculator should load up the customer information.


