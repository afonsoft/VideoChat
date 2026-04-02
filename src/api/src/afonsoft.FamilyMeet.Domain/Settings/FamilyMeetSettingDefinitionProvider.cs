using Volo.Abp.Settings;

namespace afonsoft.FamilyMeet.Settings;

public class FamilyMeetSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(FamilyMeetSettings.MySetting1));
    }
}
