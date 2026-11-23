using Voyage.Common.Constants;

namespace Voyage.Data.Seeding
{
    public class AdministratorSeeder : ConfiguredUserSeeder
    {
        protected override string ConfigurationSection => "AdminUser";

        protected override string RoleName => RoleConstants.Administrator;
    }
}
