using System;
using System.IO;
using System.Text.Json;
using NetSerializer;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using TeamServer.Models;
using TeamServer.Models.Extras;
using ApiModels.Requests;
using TeamServer.Models.Engineers;
using TeamServer.Models.Engineers.TaskResultTypes;
using System.Text;

namespace TeamServer.Utilities
{
    public static class Seralization
    {
       private static Serializer DatabaseSer { get; set; }
    
        private static IEnumerable<Type> TypesForDatabase = new List<Type>()
        {
                typeof(C2Profile),
                typeof(List<C2Profile>),
                typeof(ReconCenterEntity.ReconCenterEntityProperty),
                typeof(List<ReconCenterEntity.ReconCenterEntityProperty>),
                typeof(Dictionary<string,string>),
        };

        public static void Init()
        {
            DatabaseSer = new Serializer(TypesForDatabase);
        }

        public static byte[] Serialize<T>(this T data)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    JsonSerializer.Serialize(ms,data);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }
        
        public static byte[] ProSerialiseForDatabase<T>(this T data)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    DatabaseSer.Serialize(ms, data);
                    return ms.ToArray();
                }
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
                //Console.WriteLine(Encoding.UTF8.GetString(data));
                return JsonSerializer.Deserialize<T>(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return default;
            }
        }

        public static T ProDeserializeForDatabase<T>(this byte[] data)
        {
            try
            {
                using (var ms = new MemoryStream(data))
                {
                    return (T)DatabaseSer.Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return default;
            }
        }
    }
}
