using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using System.Linq;
using System.Diagnostics;
using System.CodeDom.Compiler;
using ApiModels.Requests;
using Microsoft.CSharp;

namespace TeamServer.Utilities
{
    public class Compile
    {

        public bool Confuse { get; set;}
        
        public static byte[] GenerateCode(string source,SpawnEngineerRequest.EngCompileType compileType)
        {
            string assemblyName = Path.GetRandomFileName();

            //replace  @"{{REPLACE_PROGRAM_NAME}}" with the name of the program you want to compile

            char allPlatformPathSeperator = Path.DirectorySeparatorChar;
            string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path D:\my_Custom_code\HardHatC2\Teamserver\ 
            pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
            pathSplit[1] = pathSplit[1].Replace("\\", allPlatformPathSeperator.ToString());
            string dataFolderPath = pathSplit[0] + "Data";

            //return the filenames from the dataFolderPath directory
            string[] assemblyRefList = Directory.GetFiles(dataFolderPath);

            EnumerationOptions enumOptions = new EnumerationOptions() { RecurseSubdirectories = true  }; // enables searching sub dirs to get all the cs files.
            string[] otherCsFileList = Directory.GetFiles(pathSplit[0] + $"..{allPlatformPathSeperator}Engineer{allPlatformPathSeperator}", "*.cs",enumOptions);
            //remove the program.cs file from the list
            string[] csFileList = otherCsFileList.Where(x => x != pathSplit[0] + $"..{allPlatformPathSeperator}Engineer{allPlatformPathSeperator}Program.cs").ToArray();
            
            //if compileType is not serviceexe then remove the ServiceExeMode.cs file from the list
            if(compileType != SpawnEngineerRequest.EngCompileType.serviceexe)
            {
                csFileList = csFileList.Where(x => x != pathSplit[0] + $"..{allPlatformPathSeperator}Engineer{allPlatformPathSeperator}Extra{allPlatformPathSeperator}ServiceExeMode.cs").ToArray();
            }

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);            
            List<SyntaxTree> trees = new List<SyntaxTree>(); //gets the other needed .cs files besides the main program.cs from Engineers folder.
            foreach (string csFile in  csFileList)
            {
                trees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(csFile)));
            }
            trees.Add(syntaxTree);
            //get Refrences from the assembly
            List<MetadataReference> references = new List<MetadataReference> { };
            // get MetadataRefrence CreateFromFile for each string in assemblyRefList
            foreach (string assembly in assemblyRefList)
            {
                MetadataReference assemblyRefrence = MetadataReference.CreateFromFile(assembly);
                references.Add(assemblyRefrence);
            }
            
            OutputKind outputKind = OutputKind.ConsoleApplication;
            if(compileType == SpawnEngineerRequest.EngCompileType.exe)
            {
                outputKind = OutputKind.ConsoleApplication;
            }
            else if(compileType == SpawnEngineerRequest.EngCompileType.dll)
            {
                outputKind = OutputKind.DynamicallyLinkedLibrary;
            }
            else if(compileType == SpawnEngineerRequest.EngCompileType.serviceexe)
            {
                outputKind = OutputKind.WindowsApplication;
            }
            else
            {
                outputKind = OutputKind.ConsoleApplication;
            }

            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, syntaxTrees: trees,
                references: references,
                options: new CSharpCompilationOptions(outputKind: outputKind, optimizationLevel: OptimizationLevel.Release, platform: Platform.X64, allowUnsafe: true));
            
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);
                    Console.WriteLine(" Roslyn Compilation failed");
                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                    return null;
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] assemblyBytes = ms.ToArray();
                    return assemblyBytes;
                }
            }
        }

        public static byte[] CompileCommands(string source)
        {
            string assemblyName = Path.GetRandomFileName();
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
            
            char allPlatformPathSeperator = Path.DirectorySeparatorChar;
            string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path D:\my_Custom_code\HardHatC2\Teamserver\ 
            pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
            pathSplit[1] = pathSplit[1].Replace("\\", allPlatformPathSeperator.ToString());
            string dataFolderPath = pathSplit[0] + "Data";

            //return the filenames from the dataFolderPath directory
            string[] assemblyRefList = Directory.GetFiles(dataFolderPath);
            
            //get Refrences from the assembly
            List<MetadataReference> references = new List<MetadataReference> { };
            foreach (string assembly in assemblyRefList)
            {
                MetadataReference assemblyRefrence = MetadataReference.CreateFromFile(assembly);
                references.Add(assemblyRefrence);
            }
            
            
            //use the roslyn compiler to read the source code
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, syntaxTrees: new[] { syntaxTree }, references:references,
                options: new CSharpCompilationOptions(outputKind: OutputKind.NetModule, optimizationLevel: OptimizationLevel.Release, platform: Platform.X64, allowUnsafe: true));
            
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
            
                // if (!result.Success)
                // {
                //     IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                //         diagnostic.IsWarningAsError ||
                //         diagnostic.Severity == DiagnosticSeverity.Error);
                //     Console.WriteLine(" Roslyn Compilation failed");
                //     foreach (Diagnostic diagnostic in failures)
                //     {
                //         Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                //     }
                //     return null;
                // }
                // else
                // {
                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] assemblyBytes = ms.ToArray();
                    return assemblyBytes;
                //}
            }
        }


        // spawn a new process running D:\Share between vms\ConfuserEx-CLI\Confuser.CLI.Exe , pass in the file location we compiled 
        public static void RunConfuser(string fileLocation)
        {
            // get the base directory of the project
            char allPlatformPathSeperator = Path.DirectorySeparatorChar;
            string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path D:\my_Custom_code\HardHatC2\Teamserver\
            pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
            pathSplit[1] = pathSplit[1].Replace("\\", allPlatformPathSeperator.ToString());

            // get the path to HardHatC2\Engineer\Engineer.crproj
            string projectPath = pathSplit[0] + $"..{allPlatformPathSeperator}Engineer{allPlatformPathSeperator}Engineer.crproj";

            Process P = new Process();
            P.StartInfo.FileName = $"D:{allPlatformPathSeperator}Share between vms{allPlatformPathSeperator}ConfuserEx-CLI{allPlatformPathSeperator}Confuser.CLI.Exe";
            // P.StartInfo.Arguments = $"\"{projectPath}\" \"-n\"";
            P.StartInfo.ArgumentList.Add($"{projectPath}");
            P.StartInfo.ArgumentList.Add("-n");
            P.Start();
        }
    }
}
