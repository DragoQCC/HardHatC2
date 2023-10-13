using HardHatC2Client.Plugin_Interfaces;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;
using ApiModels.Plugin_BaseClasses;
using ApiModels.Plugin_Interfaces;
using System.Security;

namespace HardHatC2Client.Utilities
{
    public static class HelperFunctions
    {

        public static string PlatPathSeperator = Path.DirectorySeparatorChar.ToString();
        public static string PathingTraverseUpString = ".." + PlatPathSeperator;

        public static string GetBaseFolderLocation()
        {
            string baseFolder = "";
            try
            {
                baseFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            catch
            {
                baseFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            }
            string[] basefolderarray = baseFolder.Replace("\\", PlatPathSeperator).Split("bin");
            return basefolderarray[0];
        }

        //finds the target plugin by name and returns the first one it finds or the default if it cant find the target
        public static Lazy<T1, T2> GetPluginEnumerableResult<T1, T2>(this IEnumerable<Lazy<T1, T2>> plugins, string targetName) where T2 : IClientPluginData
        {
            if (targetName == null || targetName == string.Empty)
            {
                return plugins.First(x => x.Metadata.Name.Equals("Default", StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                return plugins.FirstOrDefault(x => x.Metadata.Name.Equals(targetName, StringComparison.CurrentCultureIgnoreCase))
                    ?? plugins.First(x => x.Metadata.Name.Equals("Default", StringComparison.CurrentCultureIgnoreCase));
            }
        }

        /// <summary>
        /// Cleans empty lines from the top and bottom of data and reduces consecutive empty lines down to one line. 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The update string with fixed whitespace</returns>
        public static string RemoveDoubleEmptyLines(this string input)
        {
            string pattern = @"(?<=\S)\n{2,}(?=\S)";
            string replacement = "\n";
            return Regex.Replace(input, pattern, replacement);
        }

        public static bool IsTaskInFinishedState(ExtImplantTaskResult_Base taskRes)
        {
            if (taskRes.Status == ExtImplantTaskStatus.Complete || taskRes.Status == ExtImplantTaskStatus.Failed || taskRes.Status == ExtImplantTaskStatus.CompleteWithErrors || taskRes.Status == ExtImplantTaskStatus.FailedWithWarnings || taskRes.Status == ExtImplantTaskStatus.Cancelled)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static SecureString ToSecureString(this string plainString)
        {
            if (plainString == null)
                return null;

            SecureString secureString = new SecureString();
            foreach (char c in plainString.ToCharArray())
            {
                secureString.AppendChar(c);
            }
            return secureString;
        }
    }
}
