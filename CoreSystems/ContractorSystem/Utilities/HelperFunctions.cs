namespace HardHatCore.ContractorSystem.Utilities
{
    public static class HelperFunctions
    {
        public static string PlatPathSeperator = Path.DirectorySeparatorChar.ToString();
        public static string PathingTraverseUpString = ".." + PlatPathSeperator;

        public static string GetBasePath()
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
            string[] basefolderarray = asmbaseFolder.Replace("\\", PlatPathSeperator).Split("bin");
            return basefolderarray[0];
        }

    }
}
