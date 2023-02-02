using Microsoft.AspNetCore.Identity;
using TeamServer.Models.Database;

namespace TeamServer.Services.Extra
{
    public class MyPasswordHasher : IPasswordHasher<UserInfo>
    {
        public string HashPassword(UserInfo user, string password)
        {
            throw new System.NotImplementedException();
        }

        public PasswordVerificationResult VerifyHashedPassword(UserInfo user, string hashedPassword, string providedPassword)
        {
            //verify that the providedPassword hash matches the hashedPassword
            if(providedPassword == hashedPassword)
            {
                return PasswordVerificationResult.Success;
            }
            else
            {
                return PasswordVerificationResult.Failed;
            }
           
        }
    }
}
