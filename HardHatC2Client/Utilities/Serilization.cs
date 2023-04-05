using HardHatC2Client.Models.TaskResultTypes;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;

namespace HardHatC2Client.Utilities
{
    public static class Seralization
    {
	   
        private static IEnumerable<Type> GenericTypes = new List<Type>()
        {
            typeof(FileSystemItem),
            typeof(List<FileSystemItem>)
        };


        public static byte[] Serialise<T>(this T data)
        {
            try
            {
                return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }

        }
        

        public static T Deserialize<T>(this byte[] data)
        {
            try
            {
  
                //string json = Encoding.UTF8.GetString(data);
                //if (!string.IsNullOrEmpty(json))
                if(data.Length > 0)
                {
                    //Console.WriteLine(json);
                    return JsonSerializer.Deserialize<T>(data);
                }
                else
                {
                    return default(T);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return default(T);
            }
	       
        }
    }
}
