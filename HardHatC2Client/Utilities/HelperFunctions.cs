using System.Security;
using System.Text.RegularExpressions;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.HardHatC2Client.Plugin_Interfaces;
using ElectronNET;

namespace HardHatCore.HardHatC2Client.Utilities
{
    public static class HelperFunctions
    {

        public static string PlatPathSeperator = Path.DirectorySeparatorChar.ToString();
        public static string PathingTraverseUpString = ".." + PlatPathSeperator;
        private static string baseFolder = "";

        public static string GetBaseFolderLocation()
        {
            if(String.IsNullOrWhiteSpace(baseFolder) is false)
            {
                return baseFolder;
            }
            try
            {
                baseFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            catch
            {
                baseFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            }
            if(string.IsNullOrWhiteSpace(baseFolder) && Program.isElectron)
            {
                //use the electron api to get the base folder
                baseFolder = ElectronNET.API.Electron.App.GetAppPathAsync().Result;
            }
            Console.WriteLine($"base folder path is {baseFolder}");
            string[] basefolderarray = baseFolder.Replace("\\", PlatPathSeperator).Split("HardHatC2Client");
            baseFolder = basefolderarray[0] + PlatPathSeperator + "HardHatC2Client" + PlatPathSeperator;
            return baseFolder;
        }

        //finds the target plugin by name and returns the first one it finds or the default if it cant find the target
        public static T GetPluginEnumerableResult<T>(this IEnumerable<T> plugins, string targetName) where T : IClientPlugin
        {
            if (targetName == null || targetName == string.Empty)
            {
                return plugins.First(x => x.Name.Equals("Default", StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                return plugins.FirstOrDefault(x => x.Name.Equals(targetName, StringComparison.CurrentCultureIgnoreCase))
                    ?? plugins.First(x => x.Name.Equals("Default", StringComparison.CurrentCultureIgnoreCase));
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
