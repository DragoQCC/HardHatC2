using SQLite;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

using TeamServer.Models.Database;

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
            DatabaseService.Connection.Insert(OperatorRole);
            DatabaseService.Connection.Insert(TeamLeadRole);
            DatabaseService.Connection.Insert(AdministratorRole);
        }
        
        


    }
}
