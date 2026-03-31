using Volo.Abp.Modularity;
using Volo.Abp.AutoMapper;
using FamilyMeet.Application.Contracts;

namespace FamilyMeet.Application;

[DependsOn(
    typeof(FamilyMeetApplicationContractsModule),
    typeof(AbpAutoMapperModule)
)]
public class FamilyMeetApplicationModule : AbpModule
{
}
