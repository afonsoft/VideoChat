using Xunit;

namespace afonsoft.FamilyMeet.EntityFrameworkCore;

[CollectionDefinition(FamilyMeetTestConsts.CollectionDefinitionName)]
public class FamilyMeetEntityFrameworkCoreCollection : ICollectionFixture<FamilyMeetEntityFrameworkCoreFixture>
{

}
