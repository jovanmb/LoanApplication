using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Repositories;
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DataContext _context; 
    public Repository(DataContext context) 
    { 
        _context = context; 
    }
    public async Task<IEnumerable<T>> GetAllAsync() 
    {
        return await _context.Set<T>().ToListAsync(); 
    }
    public async Task<T> GetByIdAsync(int id) 
    { 
        return await _context.Set<T>().FindAsync(id); 
    }
    public async Task AddAsync(T entity) 
    { 
        await _context.Set<T>().AddAsync(entity); 
        await _context.SaveChangesAsync(); 
    }
    public async Task UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
    }

}
