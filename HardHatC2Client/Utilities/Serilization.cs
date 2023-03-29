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
                //var options = new DataContractJsonSerializerSettings();
                //options.KnownTypes = GenericTypes;
                //var serializer = new DataContractJsonSerializer(typeof(T));

                //using (var ms = new MemoryStream())
                //{
                //    serializer.WriteObject(ms, data);
                //    return ms.ToArray();
                //}
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
                //var options = new DataContractJsonSerializerSettings();
                //options.KnownTypes = GenericTypes;
                //var serializer = new DataContractJsonSerializer(typeof(T));
                //string json = Encoding.UTF8.GetString(data);
                //Console.WriteLine(json);
                //using (var ms = new MemoryStream(data))
                //{
                //    return (T)serializer.ReadObject(ms);
                //}
                string json = Encoding.UTF8.GetString(data);
                if (!string.IsNullOrEmpty(json))
                {
                   // Console.WriteLine(json);
                    return JsonSerializer.Deserialize<T>(json);
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
