using Volo.Abp.Modularity;

namespace afonsoft.FamilyMeet;

public abstract class FamilyMeetApplicationTestBase<TStartupModule> : FamilyMeetTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
