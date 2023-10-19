using System;
using System.IO;
using System.Text.Json;
using NetSerializer;
using System.Collections.Generic;
using HardHatCore.ApiModels.Requests;
using System.Text;
using HardHatCore.ApiModels.Shared;
using HardHatCore.TeamServer.Models.Extras;

namespace HardHatCore.TeamServer.Utilities
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
            string json = "";
            try
            {
                if (data is not null && data.Length > 0)
                {
                    json = Encoding.UTF8.GetString(data);
                    //Console.WriteLine(json);
                    if (IsValidJson(json))
                    {
                        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                        jsonSerializerOptions.AllowTrailingCommas = true;
                        return JsonSerializer.Deserialize<T>(data, jsonSerializerOptions);
                    }
                    else if (typeof(T) == typeof(string))
                    {
                        return (T)(object)json;
                    }
                    else
                    {
                        string? fixedJson =  TryToFixJson(json);
                        if (fixedJson != null)
                        {
                            return JsonSerializer.Deserialize<T>(fixedJson);
                        }
                        Console.WriteLine("Input data is not a valid JSON & is not a normal string, returning default value");
                        return default;
                    }
                }
                else
                {
                    return default;
                }

            }
            catch (Exception ex)
            {
                //Console.WriteLine(Encoding.UTF8.GetString(data));
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return default;
            }
        }

        private static bool IsValidJson(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
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

        private static string TryToFixJson(string brokenJson)
        {
            try
            {
                //this assumes the json data is a list of objects that is currently structured like {"type":"data"}{"type":"data"} instead of [{"type":"data"},{"type":"data"}]
                //this will fix the json by adding a comma between the objects and wrapping the whole thing in square brackets
                string fixedJson = brokenJson.Replace("}{", "},{");
                fixedJson = $"[{fixedJson}]";
                return fixedJson;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }
    }
}
