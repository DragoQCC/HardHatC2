using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class GetPrivs : EngineerCommand
    {
        public override string Name => "getPrivs";

        public override async Task Execute(EngineerTask task)
        {
            string output ="";
            try
            {
                int TokenInfLength = 0;
                var ThisHandle = WindowsIdentity.GetCurrent().Token;
                h_DynInv_Methods.AdvApi32FuncWrapper.GetTokenInformation(ThisHandle, h_DynInv.Win32.WinNT._TOKEN_INFORMATION_CLASS.TokenPrivileges, IntPtr.Zero, TokenInfLength, out TokenInfLength);
                var TokenInformation = Marshal.AllocHGlobal(TokenInfLength);
                if (h_DynInv_Methods.AdvApi32FuncWrapper.GetTokenInformation(WindowsIdentity.GetCurrent().Token, h_DynInv.Win32.WinNT._TOKEN_INFORMATION_CLASS.TokenPrivileges, TokenInformation, TokenInfLength, out TokenInfLength))
                {
                    var ThisPrivilegeSet = (h_DynInv.Win32.WinNT._TOKEN_PRIVILEGES)Marshal.PtrToStructure(TokenInformation, typeof(h_DynInv.Win32.WinNT._TOKEN_PRIVILEGES));
                    for (var index = 0; index < ThisPrivilegeSet.PrivilegeCount; index++)
                    {
                        var laa = ThisPrivilegeSet.Privileges[index];
                        var StrBuilder = new StringBuilder();
                        int luidNameLen = 0;
                        //var luidPointer = Marshal.AllocHGlobal(Marshal.SizeOf(laa.Luid));
                        //Marshal.StructureToPtr(laa.Luid, luidPointer, true);
                        h_DynInv_Methods.AdvApi32FuncWrapper.LookupPrivilegeName(null, ref laa.Luid, null, ref luidNameLen);
                        StrBuilder.EnsureCapacity(luidNameLen + 1);
                        if (h_DynInv_Methods.AdvApi32FuncWrapper.LookupPrivilegeName(null, ref laa.Luid, StrBuilder, ref luidNameLen))
                        {
                            var strPrivilege = StrBuilder.ToString();
                            var strAttributes = String.Format("{0}", (h_DynInv.Win32.WinNT.LuidAttributes)laa.Attributes);
                            //Marshal.FreeHGlobal(luidPointer);
                            //check that strPrivilege is not null or empty
                            if (!string.IsNullOrEmpty(strPrivilege))
                            {
                                output += strPrivilege + "|" + strAttributes + Environment.NewLine;
                            }
                        }
                    }
                }

                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(output,task,EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch(Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + ex.Message + "\n"+ ex.StackTrace,task,EngTaskStatus.Failed,TaskResponseType.String);
            }

        }
    }
}
