using LoanApplication.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace LoanApplication.Repositories;

public class CustomerRequestRepository : Repository<CustomerRequest>, ICustomerRequestRepository
{
    public CustomerRequestRepository(DataContext context) : base(context)
    {
    }

    public async Task<CustomerRequest> GetByDetailsAsync(string firstName, 
        string lastName, DateTime dateOfBirth)
    {
        return await _context.CustomerRequests
            .FirstOrDefaultAsync(c => c.FirstName == firstName && c.LastName == lastName && c.DateOfBirth == dateOfBirth);
    }
    public async Task<int> AddAsync(CustomerRequest request)
    {
        var entity = await _context.CustomerRequests.AddAsync(request);
        await _context.SaveChangesAsync();

        return entity.Entity.Id;
    }
    public async Task<CustomerRequest> GetByIdAsync(int id)
    {
        return await _context.CustomerRequests.AsNoTracking().FirstOrDefaultAsync(customer => customer.Id == id);
    }
    public async Task<int> UpdateAsync(CustomerRequest customerRequest)
    {

        _context.Entry(customerRequest).State = EntityState.Modified;
        var result = await _context.SaveChangesAsync();

        return result;
    }

}
