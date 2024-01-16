using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using HardHatCore.ApiModels.Shared;
using HardHatCore.TeamServer.Services;
using Microsoft.AspNetCore.Http;

namespace HardHatCore.TeamServer.Endpoints.LoginEndpoints
{
    public class RegisterUser : Endpoint<UserRegRequest,string>
    {
        public override void Configure()
        {
            Post("/login/register");
            Roles("Administrator");
            Options(x => x.WithTags("Login"));
        }

        public override async Task HandleAsync(UserRegRequest request, CancellationToken ct)
        {
            bool createdUser = await HardHatHub.CreateUserTS(request.Username, request.Password, request.Role);
            if (createdUser)
            {
                SendOkAsync($"User {request.Username} created with role {request.Role}"); // or any other appropriate response
            }
            else
            {
                ThrowError("failed to create user");
            }
        }
    }
}
