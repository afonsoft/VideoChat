using Microsoft.EntityFrameworkCore;
using FamiyChat.Domain.Repositories;
using FamiyChat.EntityFrameworkCore;

namespace FamiyChat.EntityFrameworkCore;

public class UnitOfWork : IUnitOfWork
{
    private readonly FamiyChatDbContext _context;

    public UnitOfWork(FamiyChatDbContext context)
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
