using System.Text;
using System.Text.Json;
using HardHatCore.HardHatC2Client.Models.TaskResultTypes;

namespace HardHatCore.HardHatC2Client.Utilities
{
    public static class Seralization
    {
	   
        private static IEnumerable<Type> GenericTypes = new List<Type>()
        {
            typeof(FileSystemItem),
            typeof(List<FileSystemItem>)
        };


        public static byte[] Serialize<T>(this T data)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    JsonSerializer.Serialize(ms, data);
                    string json = Encoding.UTF8.GetString(ms.ToArray());
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

        public static string Serialize_Str<T>(this T data)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    JsonSerializer.Serialize(ms, data);
                    string json = Encoding.UTF8.GetString(ms.ToArray());
                    return json;
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
            string json = null;
            StringBuilder concatenatedMessages = new StringBuilder();
            try
            {
                json = Encoding.UTF8.GetString(data);
                if (data.Length > 0)
                {
                    if (typeof(T) == typeof(MessageData))
                    {
                        if (IsValidJson(json))
                        {
                            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                            jsonSerializerOptions.AllowTrailingCommas = true;
                            var deserializedObject = JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
                            return deserializedObject;
                        }
                        else
                        {
                            // Split the input string by '}'
                            string[] jsonParts = json.Split(new[] { '}' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var part in jsonParts)
                            {
                                string cleanedJson = part.Trim() + "}";
                                if (IsValidJson(cleanedJson))
                                {
                                    JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                                    jsonSerializerOptions.AllowTrailingCommas = true;
                                    var deserializedObject = JsonSerializer.Deserialize<T>(cleanedJson, jsonSerializerOptions);
                                    if (deserializedObject is MessageData messageData)
                                    {
                                        concatenatedMessages.Append(messageData.Message);
                                    }
                                }
                                else if (typeof(T) == typeof(string))
                                {
                                    concatenatedMessages.Append(cleanedJson);
                                }
                                else
                                {
                                    Console.WriteLine("Input data is not a valid JSON & is not a normal string, returning default value");
                                    return default(T);
                                }
                            }
                            return (T)(object)new MessageData() { Message = concatenatedMessages.ToString() };
                        }
                    }
                    else
                    {
                        if (IsValidJson(json))
                        {
                            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                            jsonSerializerOptions.AllowTrailingCommas = true;
                            var deserializedObject = JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
                            return deserializedObject;
                        }
                        else
                        {
                            Console.WriteLine("Input data is not a valid JSON, returning default value");
                            return default(T);
                        }
                    }
                }
                Console.WriteLine("Input data is empty, returning default value");
                return default(T);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(json);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return default(T);
            }
        }

        public static T Deserialize_Str<T>(this string data)
        {
            string json = data;
            StringBuilder concatenatedMessages = new StringBuilder();
            try
            {
                if (json.Length > 0)
                {
                    if (typeof(T) == typeof(MessageData))
                    {
                        if (IsValidJson(json))
                        {
                            var deserializedObject = JsonSerializer.Deserialize<T>(json);
                            return deserializedObject;
                        }
                        else
                        {
                            // Split the input string by '}'
                            string[] jsonParts = json.Split(new[] { '}' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var part in jsonParts)
                            {
                                string cleanedJson = part.Trim() + "}";
                                if (IsValidJson(cleanedJson))
                                {
                                    JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                                    jsonSerializerOptions.AllowTrailingCommas = true;
                                    var deserializedObject = JsonSerializer.Deserialize<T>(cleanedJson, jsonSerializerOptions);
                                    if (deserializedObject is MessageData messageData)
                                    {
                                        concatenatedMessages.Append(messageData.Message);
                                    }
                                }
                                else if (typeof(T) == typeof(string))
                                {
                                    concatenatedMessages.Append(cleanedJson);
                                }
                                else
                                {
                                    Console.WriteLine("Input data is not a valid JSON & is not a normal string, returning default value");
                                    return default(T);
                                }
                            }
                            return (T)(object)new MessageData() { Message = concatenatedMessages.ToString() };
                        }
                    }
                    else
                    {
                        if (IsValidJson(json))
                        {
                            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                            jsonSerializerOptions.AllowTrailingCommas = true;
                            var deserializedObject = JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
                            return deserializedObject;
                        }
                        else
                        {
                            Console.WriteLine("Input data is not a valid JSON, returning default value");
                            return default(T);
                        }
                    }
                }
                Console.WriteLine("Input data is empty, returning default value");
                return default(T);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(json);
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
                Console.WriteLine("Input data failed is valid json check");
                return false;
            }
        }
    }
}
