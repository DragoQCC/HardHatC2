using SQLite;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using TeamServer.Models.Database;
using TeamServer.Services.Extra;
using TeamServer.Utilities;

namespace TeamServer.Services
{
    //holds all the funxtions for working with the DB in regrads to users and roles
    public class UsersRolesDatabaseService
    {
        public static async Task CreateDefaultRoles()
        {
            if(DatabaseService.Connection == null)
            {
               DatabaseService.ConnectDb();
            }

            //create some default roles if they dont exist of Operator, TeamLead, Administator, make sure they are added to the RoleInfo
            var OperatorRole = new RoleInfo { Id = Guid.NewGuid().ToString(), Name = "Operator" };
            var TeamLeadRole = new RoleInfo { Id = Guid.NewGuid().ToString(), Name = "TeamLead" };
            var AdministratorRole = new RoleInfo { Id = Guid.NewGuid().ToString(), Name = "Administrator" };
            var guestRole = new RoleInfo { Id = Guid.NewGuid().ToString(), Name = "Guest" };
            DatabaseService.Connection.Insert(OperatorRole);
            DatabaseService.Connection.Insert(TeamLeadRole);
            DatabaseService.Connection.Insert(AdministratorRole);
            DatabaseService.Connection.Insert(guestRole);
        }

        public static async Task CreateDefaultAdmin()
        {
            UserStore userStore = new UserStore();
            
            //try to get the admin user if it exists if not create it
            var adminUser = await userStore.FindByNameAsync("HardHat_Admin".Normalize().ToUpperInvariant(), new CancellationToken());
            if(adminUser != null)
            {
                return;
            }
            string AdminPass = Encryption.GenerateRandomString(20);
            var passwordHash = MyPasswordHasher.HashPassword(AdminPass, out byte[] salt);
            UserInfo user = new UserInfo { Id = Guid.NewGuid().ToString(), UserName = "HardHat_Admin", NormalizedUserName = "HardHat_Admin".Normalize().ToUpperInvariant(), PasswordHash = passwordHash };
            
            var result = await userStore.CreateAsync(user, new CancellationToken());
            await userStore.SetPasswordSaltAsync(user, salt);
            await userStore.AddToRoleAsync(user, "Administrator", new CancellationToken());
            Console.WriteLine($"[**] HardHat_Admin's password is {AdminPass}, make sure to save this password, as on the next start of the server it will not be displayed again [**]");
        }
    }
}
