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
                            return default(T);
                        }
                    }
                }
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
                return false;
            }
        }
    }
}
