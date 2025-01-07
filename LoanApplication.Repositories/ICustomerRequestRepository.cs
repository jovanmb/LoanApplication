using LoanApplication.Models;

namespace LoanApplication.Repositories;

public interface ICustomerRequestRepository : IRepository<CustomerRequest>
{
    Task<CustomerRequest> GetByDetailsAsync(string firstName, string lastName, DateTime dateOfBirth);
    Task<int> AddAsync(CustomerRequest request);
    Task<CustomerRequest> GetByIdAsync(int id);
    Task<int> UpdateAsync(CustomerRequest customerRequest);
}
