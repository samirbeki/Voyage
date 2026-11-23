using Voyage.Common.Constants;

namespace Voyage.Data.Seeding
{
    public class ModeratorSeeder : ConfiguredUserSeeder
    {
        protected override string ConfigurationSection => "ModeratorUser";

        protected override string RoleName => RoleConstants.Moderator;
    }
}
