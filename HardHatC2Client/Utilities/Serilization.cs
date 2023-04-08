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
            string json = null;
            try
            {
                json = Encoding.UTF8.GetString(data);
                if (data.Length > 0)
                {
                    if (IsValidJson(json))
                    {
                        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                        jsonSerializerOptions.AllowTrailingCommas = true;
                        return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
                    }
                    else if (typeof(T) == typeof(string))
                    {
                        json.Trim('"');
                        return (T)(object)json;
                    }
                    else
                    {
                        Console.WriteLine("Input data is not a valid JSON & is not a normal string, returning default value");
                        return default(T);
                    }
                }
                else
                {
                    return default(T);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(json);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return default(T);
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
    }
}
