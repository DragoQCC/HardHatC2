using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardHatCore.ContractorSystem.Contracts.ContractorCommTypes
{
    internal interface IExternalCommunicationDetails : ICommunicationDetails
    {
        //the function that will help transmit the data
        public Action TransmissionAction { get; set; }
        
        // can be a host name or IP address, should include port
        public string DestinationLocation { get; set; }

    }
}
