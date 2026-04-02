using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using afonsoft.FamilyMeet.Data;
using Volo.Abp.DependencyInjection;

namespace afonsoft.FamilyMeet.EntityFrameworkCore;

public class EntityFrameworkCoreFamilyMeetDbSchemaMigrator
    : IFamilyMeetDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreFamilyMeetDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the FamilyMeetDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<FamilyMeetDbContext>()
            .Database
            .MigrateAsync();
    }
}
