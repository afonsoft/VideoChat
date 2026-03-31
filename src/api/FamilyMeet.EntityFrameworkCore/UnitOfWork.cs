using Microsoft.EntityFrameworkCore;
using FamilyMeet.Domain.Repositories;
using FamilyMeet.EntityFrameworkCore;

namespace FamilyMeet.EntityFrameworkCore;

public class UnitOfWork : IUnitOfWork
{
    private readonly FamilyMeetDbContext _context;

    public UnitOfWork(FamilyMeetDbContext context)
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
