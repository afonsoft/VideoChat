using afonsoft.FamilyMeet.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace afonsoft.FamilyMeet.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class FamilyMeetController : AbpControllerBase
{
    protected FamilyMeetController()
    {
        LocalizationResource = typeof(FamilyMeetResource);
    }
}
