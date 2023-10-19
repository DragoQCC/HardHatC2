using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HardHatCore.TeamServer.Plugin_Interfaces;

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

    }
}
