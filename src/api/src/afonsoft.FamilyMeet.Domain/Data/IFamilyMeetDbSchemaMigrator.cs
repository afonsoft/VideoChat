using System.Threading.Tasks;

namespace afonsoft.FamilyMeet.Data;

public interface IFamilyMeetDbSchemaMigrator
{
    Task MigrateAsync();
}
