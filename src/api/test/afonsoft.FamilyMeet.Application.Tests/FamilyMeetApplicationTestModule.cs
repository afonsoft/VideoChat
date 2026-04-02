using Volo.Abp.Modularity;

namespace afonsoft.FamilyMeet;

[DependsOn(
    typeof(FamilyMeetApplicationModule),
    typeof(FamilyMeetDomainTestModule)
)]
public class FamilyMeetApplicationTestModule : AbpModule
{

}
