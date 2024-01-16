using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HardHatCore.TeamServer.Models.Database;
using Microsoft.AspNetCore.Identity;

namespace HardHatCore.TeamServer.Services.Extra
{
    public class UserStore : IUserStore<UserInfo>, IUserRoleStore<UserInfo>, IUserPasswordStore<UserInfo>
    {

        public async Task CheckDbConnection()
        {
            if (DatabaseService.ConnectionString == null)
            {
                DatabaseService.Init();
            }
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
                DatabaseService.CreateTables();
            }
        }

        #region IUserStore

        public async Task<IdentityResult> CreateAsync(UserInfo user, CancellationToken cancellationToken)
        {
            //check if a user with the same name already exists
            List<UserInfo> users = await DatabaseService.AsyncConnection.Table<UserInfo>().Where(x => x.UserName.ToLower() == user.UserName.ToLower()).ToListAsync();
            if (users.Count > 0)
            {
                return IdentityResult.Failed(new IdentityError { Code = "UserExists", Description = "A user with the same name already exists" });
            }

            DatabaseService.AsyncConnection.InsertAsync(user);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(UserInfo user, CancellationToken cancellationToken)
        {
            DatabaseService.AsyncConnection.DeleteAsync(user);
            return IdentityResult.Success;
        }

        public async Task<UserInfo> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            //use the database connection to find the UserInfo item by its id
            UserInfo user =  DatabaseService.AsyncConnection.Table<UserInfo>().Where(x => x.Id == userId).ToListAsync().Result[0]; // should only return 1 thing anyway as the id is unique
            return user;

        }

        public async Task<UserInfo> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            try
            {
               // List<UserInfo> testUsers = await DatabaseService.AsyncConnection.Table<UserInfo>().ToListAsync();
                UserInfo user = DatabaseService.AsyncConnection.Table<UserInfo>().Where(x => x.NormalizedUserName.ToUpper() == normalizedUserName.ToUpper()).ToListAsync().Result[0]; // should only return 1 thing anyway as the id is unique
                return user;
            }
            catch (Exception ex)
            {
                // Console.WriteLine(ex.Message);
                // Console.WriteLine(ex.StackTrace);
                return null;
            }

        }

        public async Task<string> GetNormalizedUserNameAsync(UserInfo user, CancellationToken cancellationToken)
        {
            string name = DatabaseService.AsyncConnection.Table<UserInfo>().Where(x => x == user).ToListAsync().Result[0].UserName; // this should get the user item from the table and return its name? 
            return name;
        }

        public async Task<string> GetUserIdAsync(UserInfo user, CancellationToken cancellationToken)
        {
            string Userid = DatabaseService.AsyncConnection.Table<UserInfo>().Where(x => x.Id == user.Id).ToListAsync().Result[0].Id; // this should get the user item from the table and return its name? 
            return Userid;
        }
        
        public async Task<string> GetUserNameAsync(UserInfo user, CancellationToken cancellationToken)
        {
            string name = DatabaseService.AsyncConnection.Table<UserInfo>().Where(x => x.Id == user.Id).ToListAsync().Result[0].UserName; // this should get the user item from the table and return its name? 
            return name;
        }
        
        public async Task SetNormalizedUserNameAsync(UserInfo user, string normalizedName, CancellationToken cancellationToken)
        {
            var UpdatedUser = new UserInfo { Id = user.Id,UserName = user.UserName, NormalizedUserName = normalizedName, PasswordHash = user.PasswordHash }; // you cannot update the Id this is the primary key
            DatabaseService.AsyncConnection.UpdateAsync(UpdatedUser);
        }
        
        public async Task SetUserNameAsync(UserInfo user, string userName, CancellationToken cancellationToken)
        {
            var UpdatedUser = new UserInfo { Id = user.Id, UserName = userName, PasswordHash = user.PasswordHash }; // you cannot update the Id this is the primary key
            DatabaseService.AsyncConnection.UpdateAsync(UpdatedUser);
        }

        public async Task<IdentityResult> UpdateAsync(UserInfo user, CancellationToken cancellationToken)
        {
            var UpdatedUser = new UserInfo { Id = user.Id, UserName = user.UserName, PasswordHash = user.PasswordHash }; // you cannot update the Id this is the primary key
            DatabaseService.AsyncConnection.UpdateAsync(UpdatedUser);
            return IdentityResult.Success;
        }

        public void Dispose()
        {
            //nothing to dispose 
        }
        #endregion

        #region IUserRoleStore
        public async Task AddToRoleAsync(UserInfo user, string roleName, CancellationToken cancellationToken)
        {
            var returnedRole = DatabaseService.AsyncConnection.Table<RoleInfo>().Where(x => x.Name == roleName).ToListAsync().Result[0];
            if(returnedRole == null)
            {
                RoleInfo newRole = new RoleInfo {Id= Guid.NewGuid().ToString(), Name=roleName };
                DatabaseService.AsyncConnection.InsertAsync(newRole);
                returnedRole = DatabaseService.AsyncConnection.Table<RoleInfo>().Where(x => x.Name.Equals(roleName, StringComparison.InvariantCultureIgnoreCase)).ToListAsync().Result[0];
            }
            UserRoleInfo newUserRoleInfo = new UserRoleInfo {UserRoleID = Guid.NewGuid().ToString(), RoleID = returnedRole.Id, UserID = user.Id, RoleName = roleName }; // this links up the Roles to Users 
            DatabaseService.AsyncConnection.InsertAsync(newUserRoleInfo);

        }

        public async Task<IList<string>> GetRolesAsync(UserInfo user, CancellationToken cancellationToken)
        {
            List<string> returnedRoleNames = new();
            List<UserRoleInfo> userRoles = await DatabaseService.AsyncConnection.Table<UserRoleInfo>().Where(x => x.UserID == user.Id).ToListAsync();
            foreach(UserRoleInfo roleinfo in userRoles)
            {
                returnedRoleNames.Add(roleinfo.RoleName);
            }
            return returnedRoleNames;
        }
      
        public async Task<IList<UserInfo>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            List<UserInfo> returnedUsers = new();
            List<UserRoleInfo> userRoles = await DatabaseService.AsyncConnection.Table<UserRoleInfo>().Where(x => x.RoleName == roleName).ToListAsync();
            foreach (UserRoleInfo roleinfo in userRoles)
            {
                returnedUsers.Add(DatabaseService.AsyncConnection.Table<UserInfo>().Where(x => x.Id == roleinfo.UserID).ToListAsync().Result[0]); // this should work since userId is unique per user
            }
            return returnedUsers;
        }

        public async Task<bool> IsInRoleAsync(UserInfo user, string roleName, CancellationToken cancellationToken)
        {
            List<UserRoleInfo> userRoles = await DatabaseService.AsyncConnection.Table<UserRoleInfo>().Where(x => x.UserID == user.Id).ToListAsync();
            foreach (UserRoleInfo roleinfo in userRoles)
            {
                if (roleinfo.RoleName == roleName)
                {
                    return true;
                }
            }
            return false;

        }

        public async Task RemoveFromRoleAsync(UserInfo user, string roleName, CancellationToken cancellationToken)
        {
            List<UserRoleInfo> userRoles = await DatabaseService.AsyncConnection.Table<UserRoleInfo>().Where(x => x.UserID == user.Id).ToListAsync();
            foreach (UserRoleInfo roleinfo in userRoles)
            {
                if (roleinfo.RoleName == roleName)
                {
                    DatabaseService.AsyncConnection.DeleteAsync(roleinfo); //this should work since each combo of User + role gets a unique guid for its primary key. ex. if bob is a user and an admin they would have 2 entries in the UserRoleInfo table
                }
            }

        }
        #endregion

        #region IUserPasswordStore

        public async Task<string> GetPasswordHashAsync(UserInfo user, CancellationToken cancellationToken)
        {
            UserInfo returnedUser = DatabaseService.AsyncConnection.Table<UserInfo>().Where(x => x.Id == user.Id).ToListAsync().Result[0];
            return returnedUser.PasswordHash;
        }

        public async Task<bool> HasPasswordAsync(UserInfo user, CancellationToken cancellationToken)
        {
            UserInfo returnedUser = DatabaseService.AsyncConnection.Table<UserInfo>().Where(x => x.Id == user.Id).ToListAsync().Result[0];
            if(!string.IsNullOrEmpty(returnedUser.PasswordHash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task SetPasswordHashAsync(UserInfo user, string passwordHash, CancellationToken cancellationToken)
        {
            var UpdatedUser = new UserInfo { Id = user.Id, UserName = user.UserName, PasswordHash = passwordHash }; // you cannot update the Id this is the primary key
            DatabaseService.AsyncConnection.UpdateAsync(UpdatedUser);
        }

        public async Task SetPasswordSaltAsync(UserInfo user, byte[] salt)
        {
            var userSalt = new UserSalt {Id= Guid.NewGuid().ToString() ,UserId = user.Id, Salt = salt };
            DatabaseService.AsyncConnection.InsertAsync(userSalt);
        }

        public static async Task<byte[]> GetUserPasswordSalt(string username)
        {
            try
            {
                UserInfo returnedUser = DatabaseService.AsyncConnection.Table<UserInfo>().Where(x => x.UserName.ToUpper() == username.ToUpper()).ToListAsync().Result[0];
                UserSalt returnedSalt = DatabaseService.AsyncConnection.Table<UserSalt>().Where(x => x.UserId == returnedUser.Id).ToListAsync().Result[0];
                return returnedSalt.Salt;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                byte[] dummySalt = new byte[32];
                return dummySalt;
            }
        }
        


        #endregion

    }
}
