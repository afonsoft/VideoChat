using Volo.Abp.Modularity;

namespace afonsoft.FamilyMeet;

[DependsOn(
    typeof(FamilyMeetDomainModule),
    typeof(FamilyMeetTestBaseModule)
)]
public class FamilyMeetDomainTestModule : AbpModule
{

}
