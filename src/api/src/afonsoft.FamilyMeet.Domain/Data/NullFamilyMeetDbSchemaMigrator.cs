using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace afonsoft.FamilyMeet.Data;

/* This is used if database provider does't define
 * IFamilyMeetDbSchemaMigrator implementation.
 */
public class NullFamilyMeetDbSchemaMigrator : IFamilyMeetDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
