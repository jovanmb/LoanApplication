using LoanApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Repositories;

public class DataContext : DbContext
{
    public DbSet<CustomerRequest> CustomerRequests { get; set; }
    public DataContext(DbContextOptions<DataContext> options) : base(options) 
    {
    }
}
