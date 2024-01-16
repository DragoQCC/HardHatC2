using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HardHatCore.TeamServer.Plugin_Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace HardHatCore.TeamServer.Utilities
{
    public static class Helpers
    {
        public static string PlatPathSeperator  = Path.DirectorySeparatorChar.ToString();
        public static string PathingTraverseUpString = ".." + PlatPathSeperator;

        public static string GetBaseFolderLocation()
        {
            string asmbaseFolder = "";
            try
            {
                asmbaseFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            catch
            {
                asmbaseFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            }
            string[] basefolderarray =  asmbaseFolder.Replace("\\",PlatPathSeperator).Split("bin");
            return basefolderarray[0];
        }

        //finds the target plugin by name and returns the first one it finds or the default if it cant find the target
        public static Lazy<T1, T2> GetPluginEnumerableResult<T1, T2>(this IEnumerable<Lazy<T1, T2>> plugins, string targetName) where T2 : IPluginMetadata
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

        public static T GetPluginEnumerableResult<T>(this IEnumerable<T> plugins, string targetName) where T : IPluginMetadata
        {
            if (string.IsNullOrEmpty(targetName))
            {
                // All plugins should have a name field so we can use that to search for the default plugin
                return plugins.First(x => x.Name.Equals("Default", StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                // All plugins should have a name field so we can use that to search for the default plugi
                return plugins.FirstOrDefault(x => x.Name.Equals(targetName, StringComparison.CurrentCultureIgnoreCase))
                       ?? plugins.First(x => x.Name.Equals("Default", StringComparison.CurrentCultureIgnoreCase));
            }
        }

        public static string GetUsernameFromJWT(string jwt)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var canread = handler.CanReadToken(jwt);
            if (canread is false) 
            {
                Console.WriteLine("failed to get username from jwt");
                return "";
            }
            var token = handler.ReadJwtToken(jwt);
            var username = token.Subject;
            return username;
        }

        public static T DeserializeAssetNotifValue<T>(this Dictionary<string, byte[]> assetNotifValues, string valueName)
        {
            KeyValuePair<string, byte[]> assetNotifItem =  assetNotifValues.FirstOrDefault(x => x.Key.Equals(valueName, StringComparison.CurrentCultureIgnoreCase));
            //check that the assetNotifItem has a value
            if(String.IsNullOrWhiteSpace(assetNotifItem.Key))
            {
                return default(T);
            }
            return assetNotifItem.Value.Deserialize<T>();
        }
    }
}
