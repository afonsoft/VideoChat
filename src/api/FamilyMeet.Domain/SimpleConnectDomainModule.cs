using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore;
using FamilyMeet.Domain.Shared;

namespace FamilyMeet.Domain;

[DependsOn(
    typeof(FamilyMeetDomainSharedModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class FamilyMeetDomainModule : AbpModule
{
}
