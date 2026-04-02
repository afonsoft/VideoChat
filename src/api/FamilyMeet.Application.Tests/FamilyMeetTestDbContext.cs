using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using FamilyMeet.EntityFrameworkCore;

namespace FamilyMeet.Application.Tests;

public class FamilyMeetTestDbContext : FamilyMeetDbContext
{
    public FamilyMeetTestDbContext(DbContextOptions<FamilyMeetTestDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureFamilyMeet();
    }
}
