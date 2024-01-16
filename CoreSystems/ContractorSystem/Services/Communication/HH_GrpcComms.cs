namespace HardHatCore.ContractorSystem.Services.Communication
{
    public class HH_GrpcComms
    {
        private readonly ContractSystemFactory _coreFactory;

        public HH_GrpcComms(ContractSystemFactory coreFactory)
        {
            _coreFactory = coreFactory;
        }
    }
}
