using MessagePack;
using System;
using System.IO;
using System.Text.Json;
using NetSerializer;
using System.Collections;
using System.Collections.Generic;
using TeamServer.Models;
using TeamServer.Models.Extras;

namespace TeamServer.Utilities
{
    public static class Seralization
    {
       
        public static IEnumerable<Type> GetseralTypes()
        {
            return new List<Type>()
            {
                typeof(EngineerTask),
                typeof(EngineerTaskResult),
                typeof(List<EngineerTaskResult>),
                typeof(C2TaskMessage),
                typeof(List<C2TaskMessage>),
                typeof(List<EngineerTask>),
                typeof(EngineerMetadata)
            };
        }

        //public static byte[] Serialise<T>(this T data)
        //{
        //	var serialiser = new DataContractJsonSerializer(typeof(T));

        //	using (var ms = new MemoryStream())
        //	{
        //		serialiser.WriteObject(ms, data);
        //		return ms.ToArray();
        //	}
        //}

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
            

		//public static T Deserialize<T>(this byte[] data)
		//{
		//	var serialiser = new DataContractJsonSerializer(typeof(T));

		//	using (var ms = new MemoryStream(data))
		//	{
		//		return (T)serialiser.ReadObject(ms);
		//	}
		//}

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
