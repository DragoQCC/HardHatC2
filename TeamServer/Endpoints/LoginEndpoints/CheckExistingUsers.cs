using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using HardHatCore.TeamServer.Services.Extra;
using HardHatCore.TeamServer.Utilities;
using Microsoft.AspNetCore.Http;

namespace HardHatCore.TeamServer.Endpoints.LoginEndpoints
{
    public class CheckExistingUsers : EndpointWithoutRequest<bool>
    {
        public override void Configure()
        {
            Get("/login/checkreg/{username}");
            Roles("Administrator");
            Options(x => x.WithTags("Login"));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            //check if the user exists
            string username = Route<string>("username");
            UserStore userStore = new UserStore();
            var user = await userStore.FindByNameAsync(username, new CancellationToken());
            if (user != null)
            {
                SendOkAsync(true);
            }
            else
            {
                SendNotFoundAsync();
            }
        }

    }
}
