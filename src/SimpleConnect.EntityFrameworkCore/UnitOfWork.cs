using Microsoft.EntityFrameworkCore;
using SimpleConnect.Domain.Repositories;
using SimpleConnect.EntityFrameworkCore;

namespace SimpleConnect.EntityFrameworkCore;

public class UnitOfWork : IUnitOfWork
{
    private readonly SimpleConnectDbContext _context;

    public UnitOfWork(SimpleConnectDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
