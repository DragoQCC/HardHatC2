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
using NetSerializer;

namespace Engineer
{
	public static class Extensions
	{
        public static IEnumerable<Type> GetseralTypes()
        {
            return new List<Type>()
            {
	            typeof(EngineerCommand),
                typeof(EngineerTask),
                typeof(List<EngineerTask>),
                typeof(EngineerTaskResult),
                typeof(List<EngineerTaskResult>),
                typeof(EngineerMetadata),
                typeof(List<EngineerMetadata>),
                typeof(C2TaskMessage),
                typeof(List<C2TaskMessage>),
        };
        }

        public static byte[] Serialise<T>(this T data)
		{
			try
			{
				var options = new DataContractJsonSerializerSettings();
				options.KnownTypes = new List<Type>()
				{
					typeof(FileSystemItem),
					typeof(List<FileSystemItem>)
				};
				var serializer = new DataContractJsonSerializer(typeof(T));

				using (var ms = new MemoryStream())
				{
					serializer.WriteObject(ms, data);
					return ms.ToArray();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return null;
			}
		}

        public static byte[] ProSerialise<T>(this T data)
        {
	        try
	        {
		        using (var ms = new MemoryStream())
		        {
			        var types = GetseralTypes();
			        Serializer ser = new Serializer(types);
			        ser.Serialize(ms, data);
			        return ms.ToArray();
		        }

	        }
	        catch (Exception e)
	        {
		        Console.WriteLine(e.Message);
		        Console.WriteLine(e.StackTrace);
	        }

	        return null;

        }

        public static T Deserialize<T>(this byte[] data)
		{
			try
			{
				var options = new DataContractJsonSerializerSettings();
				options.KnownTypes = new List<Type>()
				{
					typeof(FileSystemItem),
					typeof(List<FileSystemItem>)
				};
				var serializer = new DataContractJsonSerializer(typeof(T));

				using (var ms = new MemoryStream(data))
				{
					return (T)serializer.ReadObject(ms);
				}

			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return default;
			}
		}

        public static T ProDeserialize<T>(this byte[] data)
        {
	        try
	        {
		        using (var ms = new MemoryStream(data))
		        {
			        var types = GetseralTypes();
			        Serializer ser = new Serializer(types);
			        return (T)ser.Deserialize(ms);
		        }
	        }
	        catch (Exception e)
	        {
		        Console.WriteLine(e.Message);
		        Console.WriteLine(e.StackTrace);
	        }
	        return default;
        }
    }
}
