using ApiModels.Plugin_Interfaces;
using ApiModels.Shared;

namespace TeamServer.Models.TaskResultTypes
{
    public class DataChunk
    {
        public int Type { get; set; } // 1 is a part, 2 marks we hit the last of the byte[]  
        public int Position { get; set; } // Position of this chunk in the file
        public int Length { get; set; } // Size of data in bytes
        public byte[] Data { get; set; } // Actual data
        public ExtImplantTaskResponseType RealResponseType { get; set; } //this is the og response type since the task would have its reponse type updated to DataChunk
    }
}
