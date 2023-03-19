using MessagePack;
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

namespace TeamServer.Utilities
{
    public static class Seralization
    {

       private static Serializer GeneralSer { get; set; }
       private static Serializer DatabaseSer { get; set; }
    

        //made seperated lists here becasue the engineer needs an exact copy of this list and I did not want to give it copies of items that are just going in the database
        private static IEnumerable<Type> GenericTypes = new List<Type>()
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
        

        private static IEnumerable<Type> TypesForDatabase = new List<Type>()
            {
                typeof(C2Profile),
                typeof(List<C2Profile>),
                typeof(ReconCenterEntity.ReconCenterEntityProperty),
                typeof(List<ReconCenterEntity.ReconCenterEntityProperty>),
                typeof(Dictionary<string,string>),
            };
        

        public static byte[] Serialise<T>(this T data)
        {
            var options = new DataContractJsonSerializerSettings();
            options.KnownTypes = new List<Type>()
            {
                typeof(FileSystemItem),
                typeof(List<FileSystemItem>)
            };
        	var serializer = new DataContractJsonSerializer(typeof(T),options);

        	using (var ms = new MemoryStream())
        	{
        		serializer.WriteObject(ms, data);
        		return ms.ToArray();
        	}
        }
        
        public static void Init()
        {
            GeneralSer = new Serializer(GenericTypes);
            DatabaseSer = new Serializer(TypesForDatabase);
        }

        public static byte[] ProSerialise<T>(this T data)
		{
            try
            {
                using (var ms = new MemoryStream())
                {
                    GeneralSer.Serialize(ms, data);
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
            var options = new DataContractJsonSerializerSettings();
            options.KnownTypes = new List<Type>()
            {
                typeof(FileSystemItem),
                typeof(List<FileSystemItem>)
            };
            var serializer = new DataContractJsonSerializer(typeof(T),options);

        	using (var ms = new MemoryStream(data))
        	{
        		return (T)serializer.ReadObject(ms);
        	}
        }

        public static T ProDeserialize<T>(this byte[] data)
		{
            try
            {
                using (var ms = new MemoryStream(data))
                {
                    return (T)GeneralSer.Deserialize(ms);
                }

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
