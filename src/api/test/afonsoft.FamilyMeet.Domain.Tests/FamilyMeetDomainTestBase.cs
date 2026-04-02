using Volo.Abp.Modularity;

namespace afonsoft.FamilyMeet;

/* Inherit from this class for your domain layer tests. */
public abstract class FamilyMeetDomainTestBase<TStartupModule> : FamilyMeetTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
