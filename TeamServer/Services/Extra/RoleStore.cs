using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TeamServer.Models.Database;

namespace TeamServer.Services.Extra
{
    public class RoleStore : IRoleStore<RoleInfo>
    {
        public async Task CheckDbConnection()
        {
            if (DatabaseService.ConnectionString == null)
            {
                DatabaseService.Init();
            }
            if (DatabaseService.Connection == null)
            {
                DatabaseService.ConnectDb();
                DatabaseService.CreateTables();
            }
        }

        public async Task<IdentityResult> CreateAsync(RoleInfo role, CancellationToken cancellationToken)
        {
            DatabaseService.Connection.Insert(role);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(RoleInfo role, CancellationToken cancellationToken)
        {
            DatabaseService.Connection.Delete(role);
            return IdentityResult.Success;
        }

        public void Dispose()
        {
            //nothing to dispose
        }

        public async Task<RoleInfo> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            RoleInfo role = DatabaseService.Connection.Table<RoleInfo>().Where(x => x.Id == roleId).ToList()[0]; // should only return 1 thing anyway as the id is unique
            return role;

        }

        public async Task<RoleInfo> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            RoleInfo role = DatabaseService.Connection.Table<RoleInfo>().Where(x => x.Name == normalizedRoleName).ToList()[0]; // should only return 1 thing anyway as the id is unique
            return role;

        }

        public async Task<string> GetNormalizedRoleNameAsync(RoleInfo role, CancellationToken cancellationToken)
        {
            string name = DatabaseService.Connection.Table<RoleInfo>().Where(x => x == role).ToList()[0].Name; // this should get the user item from the table and return its name? 
            return name;

        }

        public async Task<string> GetRoleIdAsync(RoleInfo role, CancellationToken cancellationToken)
        {
            string id = DatabaseService.Connection.Table<RoleInfo>().Where(x => x == role).ToList()[0].Id; // this should get the user item from the table and return its name? 
            return id;

        }

        public async Task<string> GetRoleNameAsync(RoleInfo role, CancellationToken cancellationToken)
        {
            string name = DatabaseService.Connection.Table<RoleInfo>().Where(x => x == role).ToList()[0].Name; // this should get the user item from the table and return its name? 
            return name;

        }

        public async Task SetNormalizedRoleNameAsync(RoleInfo role, string normalizedName, CancellationToken cancellationToken)
        {
            role.Name = normalizedName;
            DatabaseService.Connection.Update(role);

        }

        public async Task SetRoleNameAsync(RoleInfo role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            DatabaseService.Connection.Update(role);

        }

        public async Task<IdentityResult> UpdateAsync(RoleInfo role, CancellationToken cancellationToken)
        {
            DatabaseService.Connection.Update(role);
            return IdentityResult.Success;

        }
    }
}
