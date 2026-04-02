using System;
using System.Collections.Generic;
using System.Text;
using afonsoft.FamilyMeet.Localization;
using Volo.Abp.Application.Services;

namespace afonsoft.FamilyMeet;

/* Inherit your application services from this class.
 */
public abstract class FamilyMeetAppService : ApplicationService
{
    protected FamilyMeetAppService()
    {
        LocalizationResource = typeof(FamilyMeetResource);
    }
}
