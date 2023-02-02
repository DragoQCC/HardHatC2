using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Functions
{
    public class DownloadTracker
    {

       public static Dictionary<string,List<string>> _downloadedFileParts = new Dictionary<string, List<string>>();


        public static void SplitFileString(string filename,string b64file)
        {
            //split the b64string into parts of 500 kb in size and add them to the _downloadedFileParts list the key is the filename and the value is the b64string
            int partSize = 500000;
            int partCount = b64file.Length / partSize;
            if (b64file.Length % partSize != 0)
            {
                partCount++;
            }
            for (int i = 1; i <= partCount; i++)
            {
                int startIndex = 0;
                int length = partSize;
                if (startIndex + length > b64file.Length)
                {
                    length = b64file.Length - startIndex;
                }
                string part = b64file.Substring(startIndex, length);
                if (!_downloadedFileParts.ContainsKey(filename))
                {
                    _downloadedFileParts.Add(filename, new List<string>());
                }
                if (_downloadedFileParts.ContainsKey(filename))
                {
                    //append the string Section{startindex}//{partCount} to the end of the part
                    part += $"PART{i}/{partCount}PARTS";
                    _downloadedFileParts[filename].Add(part);
                }
            }
        }
        
    }
}
