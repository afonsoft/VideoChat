using afonsoft.FamilyMeet.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace afonsoft.FamilyMeet.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(FamilyMeetEntityFrameworkCoreModule),
    typeof(FamilyMeetApplicationContractsModule)
    )]
public class FamilyMeetDbMigratorModule : AbpModule
{
}
