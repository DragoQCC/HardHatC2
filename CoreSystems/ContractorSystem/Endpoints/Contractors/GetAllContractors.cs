using FastEndpoints;
using HardHatCore.ContractorSystem.Contractors.ContractorTypes;

namespace HardHatCore.ContractorSystem.Endpoints.Contractors
{
    public class GetAllContractors : EndpointWithoutRequest<IEnumerable<IContractor>>
    {
        public override void Configure()
        {
            Get("/Contractors");
            AllowAnonymous();
            Options(x => x.WithTags("Contractors"));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var contractors = await Services.ContractSystemFactory.GetContractors();
            await SendAsync(contractors);
        }
    }
}
