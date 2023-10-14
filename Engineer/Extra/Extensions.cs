using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Engineer.Commands;
using Engineer.Models;
using fastJSON;

namespace Engineer
{
	public static class Extensions
	{

		public static byte[] JsonSerialize<T>(this T data)
		{
            try
            {
                JSONParameters jsonParameters = new JSONParameters();
                jsonParameters.UseValuesOfEnums = true;
                jsonParameters.UseExtensions = false;

                object dataToSerialize;

                if (typeof(T) == typeof(string))
                {
                    dataToSerialize = new MessageData{ Message = (string)(object)data};
                }
                else
                {
                    dataToSerialize = data;
                }

                string json = JSON.ToNiceJSON(dataToSerialize, jsonParameters);
                //Console.WriteLine(json);

                //write the json string to a memory stream and return the byte array
                using (MemoryStream ms = new MemoryStream())
                {
                    using (StreamWriter sw = new StreamWriter(ms))
                    {
                        sw.Write(json);
                        sw.Flush();
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        public static T JsonDeserialize<T>(this byte[] data)
        {
            try
            {
                string json = Encoding.UTF8.GetString(data);
                //Console.WriteLine(json);
                //if data type is string, return the message data
                if (typeof(T) == typeof(string))
                {
                    MessageData messageData = JSON.ToObject<MessageData>(json);
                    return (T)(object)messageData.Message;
                }
                return JSON.ToObject<T>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return default;
            }
        }
    }
}
