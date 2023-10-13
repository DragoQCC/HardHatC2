using DynamicEngLoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Modules
{
    [Module("DataChunk")]
    public class ChunkDataModule
    {
        public static List<DataChunk> ChunkData(byte[] data, int chunkSize, TaskResponseType realResponseType)
        {
            try
            {
                Console.WriteLine($"Chunking data of size {data.Length} into chunks of size {chunkSize}");
                var DataChunks = new List<DataChunk>();
                var offset = 0;
                var position = 0;
                while (offset < data.Length)
                {
                    position++;
                    var size = Math.Min(chunkSize, data.Length - offset);
                    var chunk = new byte[size];
                    Array.Copy(data, offset, chunk, 0, size);
                    DataChunks.Add(new DataChunk { Type = 1,Position = position, Length = chunk.Length, Data = chunk, RealResponseType = realResponseType });
                    Console.WriteLine($"Added type 1 chunk with postion {position}, and length {chunk.Length}");
                    offset += size;
                }
                DataChunks.Add(new DataChunk { Type = 2,Position= position+1, Length = 0, Data = new byte[0], RealResponseType = realResponseType });
                Console.WriteLine($"Added type 2 chunk with postion {position}");
                Console.WriteLine($"Chunked data into {DataChunks.Count} chunks");
                return DataChunks;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }
    }
}
