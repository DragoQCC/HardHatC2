using DynamicEngLoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Functions
{
    public class DownloadTracker
    {
        public static List<FilePart> SplitFileString(byte[] fileContent)
        {
            //split the b64string into parts of 500 kb in size and add them to the _downloadedFileParts list the key is the filename and the value is the b64string
            int size = Program.DownloadChunkSize;
            List<FilePart> parts = new List<FilePart>();

            for (int i = 0; i < fileContent.Length; i += size)
            {
                int length = Math.Min(size, fileContent.Length - i);
                byte[] part = new byte[length];
                Array.Copy(fileContent, i, part, 0, length);
                parts.Add(new FilePart { Type = 1, Length = length, Data = part });
            }
            parts.Add(new FilePart { Type = 2, Length = 0, Data = new byte[0] });
            return parts;
        }
        
    }
}
