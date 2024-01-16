namespace HardHatCore.ContractorSystem.Contractors.ContractorCommTypes
{
    public interface ICommunicationDetails
    {
        //the function that will help transmit the data
        public Action TransmissionAction { get; set; }

        // can be a host name or IP address, should include port
        public string DestinationLocation { get; set; }

    }


}