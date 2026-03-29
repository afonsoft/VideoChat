using Microsoft.EntityFrameworkCore;
using FamilyChat.Domain.Repositories;
using FamilyChat.EntityFrameworkCore;

namespace FamilyChat.EntityFrameworkCore;

public class UnitOfWork : IUnitOfWork
{
    private readonly FamilyChatDbContext _context;

    public UnitOfWork(FamilyChatDbContext context)
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
