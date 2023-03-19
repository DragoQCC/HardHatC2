using HardHatC2Client.Models.TaskResultTypes;
using System.Runtime.Serialization.Json;

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
	        var options = new DataContractJsonSerializerSettings();
	        options.KnownTypes = GenericTypes;
	        var serializer = new DataContractJsonSerializer(typeof(T));

        	using (var ms = new MemoryStream())
        	{
        		serializer.WriteObject(ms, data);
        		return ms.ToArray();
        	}
        }
        

        public static T Deserialize<T>(this byte[] data)
        {
	        var options = new DataContractJsonSerializerSettings();
	        options.KnownTypes = GenericTypes;
	        var serializer = new DataContractJsonSerializer(typeof(T));

        	using (var ms = new MemoryStream(data))
        	{
        		return (T)serializer.ReadObject(ms);
        	}
        }
    }
}
