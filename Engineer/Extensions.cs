using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
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
			var serialiser = new DataContractJsonSerializer(typeof(T));

			using (var ms = new MemoryStream())
			{
				serialiser.WriteObject(ms, data);
				return ms.ToArray();
			}

		}

        public static byte[] ProSerialise<T>(this T data)
        {
            using (var ms = new MemoryStream())
            {
                var types = GetseralTypes();
                Serializer ser = new Serializer(types);
                ser.Serialize(ms, data);
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(this byte[] data)
		{
			var serialiser = new DataContractJsonSerializer(typeof(T));

			using (var ms = new MemoryStream(data))
			{
				return (T)serialiser.ReadObject(ms);
			}
        }

        public static T ProDeserialize<T>(this byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var types = GetseralTypes();
                Serializer ser = new Serializer(types);
                return (T)ser.Deserialize(ms);
            }
        }
    }
}
