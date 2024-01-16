using System;
using System.Collections.Generic;


namespace HardHatCore.TeamServer.Utilities;


public class MergeAssembly
{
    public static void MergeAssemblies(string outfileName, string[] sources,string searchdir)
    {
        try
        {
            var repack = new ILRepacking.ILRepack(new ILRepacking.RepackOptions
            {
                InputAssemblies = sources,
                OutputFile = outfileName,
                SearchDirectories = new List<string> { searchdir },
                XmlDocumentation = false,
                DebugInfo = false,
                TargetKind = ILRepacking.ILRepack.Kind.SameAsPrimaryAssembly,
                
            });
            repack.Repack();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }
}