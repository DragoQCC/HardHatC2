namespace HardHatCore.ContractorSystem.Contractors.ContractorCommTypes
{
    public interface ICommunicationDetails
    {
        public ContractorCommMethod CommMethod { get; set; }

    }

    public enum ContractorCommMethod
    {
        Custom,
        SignalR,
        WebSocket,
        Grpc,
    }


}