using SQLite;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime;
using HardHatCore.TeamServer.Models.Database;
using HardHatCore.TeamServer.Services.Extra;
using HardHatCore.TeamServer.Utilities;

namespace HardHatCore.TeamServer.Services
{
    //holds all the funxtions for working with the DB in regrads to users and roles
    public class UsersRolesDatabaseService
    {
        public static async Task CreateDefaultRoles()
        {
            if(DatabaseService.AsyncConnection == null)
            {
               DatabaseService.ConnectDb();
            }

            //create some default roles if they dont exist of Operator, TeamLead, Administator, make sure they are added to the RoleInfo
            var OperatorRole = new RoleInfo { Id = Guid.NewGuid().ToString(), Name = "Operator" };
            var TeamLeadRole = new RoleInfo { Id = Guid.NewGuid().ToString(), Name = "TeamLead" };
            var AdministratorRole = new RoleInfo { Id = Guid.NewGuid().ToString(), Name = "Administrator" };
            var guestRole = new RoleInfo { Id = Guid.NewGuid().ToString(), Name = "Guest" };
            DatabaseService.AsyncConnection.InsertAsync(OperatorRole);
            DatabaseService.AsyncConnection.InsertAsync(TeamLeadRole);
            DatabaseService.AsyncConnection.InsertAsync(AdministratorRole);
            DatabaseService.AsyncConnection.InsertAsync(guestRole);
        }

        public static async Task CreateDefaultAdmin()
        {
            UserStore userStore = new UserStore();
            var AdminUsername = Environment.GetEnvironmentVariable("HARDHAT_ADMIN_USERNAME") ?? "HardHat_Admin";

            //try to get the admin user if it exists if not create it
            var adminUser = await userStore.FindByNameAsync("HardHat_Admin".Normalize().ToUpperInvariant(), new CancellationToken());
            if(adminUser != null)
            {
                return;
            }
            string AdminPass = Environment.GetEnvironmentVariable("HARDHAT_ADMIN_PASSWORD") ?? Encryption.GenerateRandomString(20);
            var passwordHash = MyPasswordHasher.HashPassword(AdminPass, out byte[] salt);
            UserInfo user = new UserInfo { Id = Guid.NewGuid().ToString(), UserName = AdminUsername, NormalizedUserName = AdminUsername.Normalize().ToUpperInvariant(), PasswordHash = passwordHash };
            
            var result = await userStore.CreateAsync(user, new CancellationToken());
            await userStore.SetPasswordSaltAsync(user, salt);
            await userStore.AddToRoleAsync(user, "Administrator", new CancellationToken());
            Console.WriteLine($"[**] {AdminUsername}'s password is {AdminPass}, make sure to save this password, as on the next start of the server it will not be displayed again [**]");

            // If password not set via environment variable, print randomly generated password
            bool gotPasswordFromEnv = Environment.GetEnvironmentVariable("HARDHAT_ADMIN_PASSWORD") != null;
            if (!gotPasswordFromEnv)
            {
                Console.WriteLine($"[**] Default admin account; SAVE THIS PASSWORD; it will not be displayed again [**]\n    Username: {AdminUsername}\n    Password: {AdminPass}");
            }
        }
    }
}
