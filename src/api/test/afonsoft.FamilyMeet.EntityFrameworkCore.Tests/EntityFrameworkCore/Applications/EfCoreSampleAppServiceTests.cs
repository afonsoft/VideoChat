using afonsoft.FamilyMeet.Samples;
using Xunit;

namespace afonsoft.FamilyMeet.EntityFrameworkCore.Applications;

[Collection(FamilyMeetTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<FamilyMeetEntityFrameworkCoreTestModule>
{

}
