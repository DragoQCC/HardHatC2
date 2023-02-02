using MessagePack;
using System;
using System.IO;
using System.Text.Json;
using NetSerializer;
using System.Collections;
using System.Collections.Generic;
using TeamServer.Models;
using TeamServer.Models.Extras;
using ApiModels.Requests;

namespace TeamServer.Utilities
{
    public static class Seralization
    {


        //made seperated lists here becasue the engineer needs an exact copy of this list and I did not want to give it copies of items that are just going in the database
        public static IEnumerable<Type> GetseralTypesForEngineers()
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

        public static IEnumerable<Type> GetSeralTypesForDatabase()
        {
            return new List<Type>()
            {
                typeof(C2Profile),
                typeof(List<C2Profile>),
                typeof(ReconCenterEntity.ReconCenterEntityProperty),
                typeof(List<ReconCenterEntity.ReconCenterEntityProperty>),
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
            try
            {
                using (var ms = new MemoryStream())
                {
                    var types = GetseralTypesForEngineers();
                    Serializer ser = new Serializer(types);
                    ser.Serialize(ms, data);
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
                    var types = GetSeralTypesForDatabase();
                    Serializer ser = new Serializer(types);
                    ser.Serialize(ms, data);
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
            try
            {
                using (var ms = new MemoryStream(data))
                {
                    var types = GetseralTypesForEngineers();
                    Serializer ser = new Serializer(types);
                    return (T)ser.Deserialize(ms);
                    
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
                    var types = GetSeralTypesForDatabase();
                    Serializer ser = new Serializer(types);
                    return (T)ser.Deserialize(ms);

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
