using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using HardHatCore.TeamServer.Models.Database;
using HardHatCore.TeamServer.Services.Extra;
using HardHatCore.TeamServer.Services;
using Microsoft.AspNetCore.Http;
using System;

namespace HardHatCore.TeamServer.Endpoints.LoginEndpoints
{
    public class LoginUser : Endpoint<LoginRequest,string>
    {
        public override void Configure()
        {
            Post("/login");
            AllowAnonymous();
            Options(x => x.WithTags("Login"));
        }

        public override async Task HandleAsync(LoginRequest request, CancellationToken ct)
        {
            //create an instance of the UserStore and check if the user exists
            UserStore userStore = new UserStore();
            var user = await userStore.FindByNameAsync(request.Username, new CancellationToken());
            if (user != null)
            {
                string PasswordHash = "";
                if (request.PasswordHash is null)
                {
                    byte[] salt = await UserStore.GetUserPasswordSalt(request.Username);
                    PasswordHash = Utilities.Hash.HashPassword(request.Password, salt);
                }
                else
                {
                    PasswordHash = request.PasswordHash;
                }
                string token = await Authentication.SignIn(user, PasswordHash);
                //if the user exists check if the password hash matches the one in the store
                if (!string.IsNullOrEmpty(token))
                {
                    //sign was successful 
                    SendOkAsync(token);
                }
                else
                {
                    //sign in failed
                    Console.WriteLine("sign in failed double check creds");
                    ThrowError("Sign in failed, check ts logs / console for details");
                }
            }
            else
            {
                //if the user does not exist return false
                Console.WriteLine("sign in failed, user not found");
                ThrowError("Sign in failed, check ts logs / console for details");
            }
        }
    }
}
