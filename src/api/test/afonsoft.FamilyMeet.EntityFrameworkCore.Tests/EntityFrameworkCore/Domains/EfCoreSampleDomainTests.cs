using afonsoft.FamilyMeet.Samples;
using Xunit;

namespace afonsoft.FamilyMeet.EntityFrameworkCore.Domains;

[Collection(FamilyMeetTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<FamilyMeetEntityFrameworkCoreTestModule>
{

}
