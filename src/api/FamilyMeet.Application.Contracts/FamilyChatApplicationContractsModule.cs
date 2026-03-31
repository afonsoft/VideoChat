using Volo.Abp.Modularity;
using FamilyMeet.Domain.Shared;

namespace FamilyMeet.Application.Contracts;

[DependsOn(
    typeof(FamilyMeetDomainSharedModule)
)]
public class FamilyMeetApplicationContractsModule : AbpModule
{
}
