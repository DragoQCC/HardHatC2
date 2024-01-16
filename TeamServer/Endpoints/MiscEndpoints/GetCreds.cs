using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using HardHatCore.TeamServer.Models.Extras;
using Microsoft.AspNetCore.Http;

namespace HardHatCore.TeamServer.Endpoints.MiscEndpoints
{
    public class GetCreds : EndpointWithoutRequest<IEnumerable<Cred>>
    {
        public override void Configure()
        {
            Get("/misc/getcreds");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Cred"));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            await SendAsync(Cred.CredList);
        }
    }
}
