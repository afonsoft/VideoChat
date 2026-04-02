using Microsoft.Extensions.Localization;
using afonsoft.FamilyMeet.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace afonsoft.FamilyMeet;

[Dependency(ReplaceServices = true)]
public class FamilyMeetBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<FamilyMeetResource> _localizer;

    public FamilyMeetBrandingProvider(IStringLocalizer<FamilyMeetResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
