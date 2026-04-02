using afonsoft.FamilyMeet.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace afonsoft.FamilyMeet.Permissions;

public class FamilyMeetPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(FamilyMeetPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(FamilyMeetPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<FamilyMeetResource>(name);
    }
}
