﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using System.Linq;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Reflection.PortableExecutable;
using ApiModels.Requests;
using Microsoft.CSharp;
using System.Security.Cryptography.Xml;
using ApiModels.Shared;
using Mono.Cecil;


namespace TeamServer.Utilities
{
    public class Compile
    {

        public bool Confuse { get; set;}
        
        public static byte[] GenerateEngCode(string source, EngCompileType compileType, SleepTypes sleepType, List<string> nonIncCommandList, List<string> nonIncModuleList)
        {
            bool IsEngDynLibCompiled = CompileEngDynamicLibrary();
            if (IsEngDynLibCompiled)
            {
                Console.WriteLine("Successfully compiled the DynamicEngLoading.dll");
            }
            else
            {
                Console.WriteLine("Failed to compile the DynamicEngLoading.dll");
            }
            string assemblyName = Path.GetRandomFileName();

            //replace  @"{{REPLACE_PROGRAM_NAME}}" with the name of the program you want to compile

            char allPlatformPathSeperator = Path.DirectorySeparatorChar;
            string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path HardHatC2\Teamserver\ 
            pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
            pathSplit[1] = pathSplit[1].Replace("\\", allPlatformPathSeperator.ToString());
            string dataFolderPath = pathSplit[0] + "Data";

            //return the filenames from the dataFolderPath directory
            string[] assemblyRefList = Directory.GetFiles(dataFolderPath);

            //get the shared library location 
            string TopLevelFolder = pathSplit[0] + $"..{allPlatformPathSeperator}";

            EnumerationOptions enumOptions = new EnumerationOptions() { RecurseSubdirectories = true  }; // enables searching sub dirs to get all the cs files.
            string[] otherCsFileList = Directory.GetFiles(pathSplit[0] + $"..{allPlatformPathSeperator}Engineer{allPlatformPathSeperator}", "*.cs",enumOptions);
            //remove the program.cs file from the list
            string[] csFileList = otherCsFileList.Where(x => x != pathSplit[0] + $"..{allPlatformPathSeperator}Engineer{allPlatformPathSeperator}Program.cs").ToArray();
            
            //if compileType is not serviceexe then remove the ServiceExeMode.cs file from the list
            if(compileType != EngCompileType.serviceexe)
            {
                csFileList = csFileList.Where(x => x != pathSplit[0] + $"..{allPlatformPathSeperator}Engineer{allPlatformPathSeperator}Extra{allPlatformPathSeperator}ServiceExeMode.cs").ToArray();
            }

            //if a csFileList item has the DynamicCommands folder in it then remove it from the list
            csFileList = csFileList.Where(x => !x.Contains("DynamicCommands")).ToArray();
            
            //remove the non included command files from the csFileList
            csFileList = csFileList.Where(x => !nonIncCommandList.Contains(x)).ToArray();
            //remove the module files from the csFileList
            csFileList = csFileList.Where(x => !nonIncModuleList.Contains(x)).ToArray();
           
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
            if(compileType == EngCompileType.exe)
            {
                outputKind = OutputKind.ConsoleApplication;
            }
            else if(compileType == EngCompileType.dll)
            {
                outputKind = OutputKind.DynamicallyLinkedLibrary;
            }
            else if(compileType == EngCompileType.serviceexe)
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

                        // Print the code that caused the error
                        var lineSpan = diagnostic.Location.GetLineSpan();
                        var startLine = lineSpan.StartLinePosition.Line;
                        var endLine = lineSpan.EndLinePosition.Line;
                        var sourceText = lineSpan.Path == "" ? source : File.ReadAllText(lineSpan.Path);
                        var sourceLines = sourceText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                        for (int i = startLine; i <= endLine; i++)
                        {
                            Console.Error.WriteLine("Line {0}: {1}", i + 1, sourceLines[i]);
                        }
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
            char allPlatformPathSeperator = Path.DirectorySeparatorChar;
            string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path HardHatC2\Teamserver\ 
            pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
            pathSplit[1] = pathSplit[1].Replace("\\", allPlatformPathSeperator.ToString());
            //string dataFolderPath = pathSplit[0] + "Data";
            string TopLevelFolder = pathSplit[0] + $"..{allPlatformPathSeperator}";
           // string DynamicLoadingDllPath = TopLevelFolder + "DynamicEngLoading" +allPlatformPathSeperator + "bin" + allPlatformPathSeperator+"Debug";
            //in the TopLeelFolder should be files in the format Engineer_randomStrings.exe, we need one of those file paths 
          // string[] DynamicLoadingLibrary = Directory.GetFiles(DynamicLoadingDllPath, "DynamicEngLoading.dll");

            string dataFolderPath = pathSplit[0] + "Data" + allPlatformPathSeperator + "NewCommandStandard";

            //return the filenames from the dataFolderPath directory
            string[] assemblyRefList = Directory.GetFiles(dataFolderPath);

            List<MetadataReference> referencedAssemblies = new List<MetadataReference>
            {
               // MetadataReference.CreateFromFile($"{DynamicLoadingLibrary[0]}")
            };
            foreach (string assembly in assemblyRefList)
            {
                MetadataReference assemblyRefrence = MetadataReference.CreateFromFile(assembly);
                referencedAssemblies.Add(assemblyRefrence);
            }

            //EnumerationOptions enumOptions = new EnumerationOptions() { RecurseSubdirectories = true }; // enables searching sub dirs to get all the cs files.
            //string[] csFileList = otherCsFileList.Where(x => x != pathSplit[0] + $"..{allPlatformPathSeperator}Engineer{allPlatformPathSeperator}*_CommModule.cs").ToArray();

            // Create the syntax tree
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
            List<SyntaxTree> trees = new List<SyntaxTree>(); //gets the other needed .cs files besides the main program.cs from Engineers folder.
            //foreach (string csFile in otherCsFileList)
            //{
            //    trees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(csFile)));
            //}
            trees.Add(syntaxTree);

            // Set up the compilation options
            CSharpCompilationOptions options = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, platform: Platform.X64, allowUnsafe: true);

            
            // Create the compilation
            CSharpCompilation compilation = CSharpCompilation.Create("GeneratedAssembly.dll", trees, referencedAssemblies, options);

            
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

                        // Print the code that caused the error
                        var lineSpan = diagnostic.Location.GetLineSpan();
                        var startLine = lineSpan.StartLinePosition.Line;
                        var endLine = lineSpan.EndLinePosition.Line;
                        var sourceText = lineSpan.Path == "" ? source : File.ReadAllText(lineSpan.Path);
                        var sourceLines = sourceText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                        for (int i = startLine; i <= endLine; i++)
                        {
                            Console.Error.WriteLine("Line {0}: {1}", i + 1, sourceLines[i]);
                        }
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


        // spawn a new process running D:\Share between vms\ConfuserEx-CLI\Confuser.CLI.Exe , pass in the file location we compiled 
        public static void RunConfuser(string fileLocation)
        {
            // get the base directory of the project
            char allPlatformPathSeperator = Path.DirectorySeparatorChar;
            string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path 
            pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
            pathSplit[1] = pathSplit[1].Replace("\\", allPlatformPathSeperator.ToString());

            // get the path to HardHatC2\Engineer\Engineer.crproj
            string projectPath = pathSplit[0] + $"..{allPlatformPathSeperator}Engineer{allPlatformPathSeperator}Engineer.crproj";

           // Process P = new Process();
           // P.StartInfo.FileName =
            // P.StartInfo.Arguments = $"\"{projectPath}\" \"-n\"";
           // P.StartInfo.ArgumentList.Add($"{projectPath}");
          //  P.StartInfo.ArgumentList.Add("-n");
           // P.Start();
        }

        public static bool CompileEngDynamicLibrary()
        {
            char allPlatformPathSeperator = Path.DirectorySeparatorChar;
            string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path
            pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
            string dynamicEngFolderLocation = pathSplit[0] + $"..{allPlatformPathSeperator}DynamicEngLoading{allPlatformPathSeperator}";
            List<string> dynamicEngFilesLocation  = Directory.GetFiles(dynamicEngFolderLocation, "*.cs", new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive}).ToList();

            List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
            foreach (string file in dynamicEngFilesLocation)
            {
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(file)));
            }
            //get Refrences from the assembly
            List<MetadataReference> references = new List<MetadataReference> { };

            string[] assemblyRefList = Directory.GetFiles(pathSplit[0]+"Data"+allPlatformPathSeperator+"DynamicEngDll");

            foreach (string assembly in assemblyRefList)
            {
                MetadataReference assemblyRefrence = MetadataReference.CreateFromFile(assembly);
                references.Add(assemblyRefrence);
            }

            CSharpCompilation compilation = CSharpCompilation.Create(Path.GetRandomFileName(), syntaxTrees: syntaxTrees,
                references: references,
                options: new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, platform: Platform.X64, allowUnsafe: true));

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

                        // Print the code that caused the error
                        var lineSpan = diagnostic.Location.GetLineSpan();
                        var startLine = lineSpan.StartLinePosition.Line;
                        var endLine = lineSpan.EndLinePosition.Line;

                        for (int i = startLine; i <= endLine; i++)
                        {
                            //Console.Error.WriteLine("Line {0}: {1}", i + 1, sourceLines[i]);
                        }
                    }
                    return false;
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] assemblyBytes = ms.ToArray();
                    //should help to remove file handles on the dll
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    File.WriteAllBytes(pathSplit[0] + allPlatformPathSeperator + "Data" + allPlatformPathSeperator+"loading.dll", assemblyBytes);
                    return true;
                }
            }
        }
    }
}
