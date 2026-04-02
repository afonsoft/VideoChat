using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Autofac;
using FamilyMeet.EntityFrameworkCore;
using FamilyMeet.Application;

namespace FamilyMeet.Application.Tests;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(FamilyMeetApplicationModule),
    typeof(FamilyMeetEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule),
     typeof(FamilyMeetEntityFrameworkCoreModule)
)]
public class FamilyMeetApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddEntityFrameworkInMemoryDatabase<FamilyMeetTestDbContext>();
        
        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(abpDbContextConfigurationContext =>
            {
                abpDbContextConfigurationContext.DbContextOptions.UseInMemoryDatabase("FamilyMeetTestDb");
            });
        });
    }
}
