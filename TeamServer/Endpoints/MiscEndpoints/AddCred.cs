using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Services;
using Microsoft.AspNetCore.Http;

namespace HardHatCore.TeamServer.Endpoints.MiscEndpoints
{
    public class AddCred : Endpoint<Cred>
    {
        public override void Configure()
        {
            Post("/misc/addcred");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Cred"));
        }

        public override async Task HandleAsync(Cred req, CancellationToken ct)
        {
            try
            {
                await HardHatHub.AddCreds(new List<Cred>() { req }, true);
                SendOkAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in add cred api: " + ex.Message);
                ThrowError("error in add cred api: " + ex.Message);
            }
        }
    }
}
