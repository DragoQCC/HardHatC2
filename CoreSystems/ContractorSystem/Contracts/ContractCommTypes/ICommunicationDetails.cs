namespace HardHatCore.ContractorSystem.Contracts.ContractorCommTypes
{
    public interface ICommunicationDetails
    {
        //prob wont get any values but is being used as a marker interface
        public CommLocation _commLocation { get; set; }
    }

    public enum CommLocation
    {
        Client,
        TeamServer,
        External
    }
}