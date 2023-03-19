namespace HardHatC2Client.Utilities
{
    public class Help
    {
       public static List<HelpMenuItem> menuItems = HelpMenuItem.itemList;        
        public static List<HelpMenuItem> DisplayHelp(Dictionary<string,string> input)
        {
            //string output = "";
            //if (!input.TryGetValue("/command",out var command))
            //{
            //    //add the Name, Description and Keys of each item to the output as long as they are not null

            //    foreach (HelpMenuItem item in menuItems)
            //    {
            //        output += $"\nName: {item.Name} | Desc: {item.Description}\n";
            //        output += $" | Usage:\n     {item.Usage}";
            //    }
            //    return output;
            //}
            ////else if input has value find the matching command by name and print just its info
            //else
            //{
            //    foreach (HelpMenuItem item in menuItems)
            //    {
            //        if (item.Name.ToLower() == command.ToLower())
            //        {
            //            output += $"-Name: {item.Name}\n";
            //            output += $"    -Desc: {item.Description}\n";
            //            output += $"    -Usage: {item.Usage}\n";
            //            output += $"    -Needs Admin: {item.NeedsAdmin}\n";
            //            output += $"    -Opsec Risk Status: {item.Opsec}\n";
            //            output += $"    -Details: {item.Details}\n";
            //            if (!string.IsNullOrEmpty(item.Keys))
            //            {
            //                output += $"    -Keys:\n     {item.Keys}\n";
            //            }
            //            output += "\n";
            //            return output;
            //        }
            //    }
            //    return "Command not found";
            //}

            if (!input.TryGetValue("/command", out var command))
            {
                return menuItems;
            }
            else
            {
                List<HelpMenuItem> output = new List<HelpMenuItem>();
                foreach (HelpMenuItem item in menuItems)
                {
                    if (item.Name.ToLower() == command.ToLower())
                    {
                        output.Add(item);
                    }
                }
                return output;
            }
        }



        public class HelpMenuItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Usage { get; set; }
            public bool NeedsAdmin { get; set; }

            public OpsecStatus Opsec { get; set; }
            public string Details { get; set; }
            public string Keys { get; set; }

            //enum of opsec status 
            public enum OpsecStatus
            {
                NotSet,
                Low,
                Moderate,
                High,
                RequiresLeadAuthorization,
                Blocked
            }

            public static List<HelpMenuItem> itemList = new List<HelpMenuItem>
            {
                //new HelpMenuItem()
                //{
                //    Name = "template",
                //    Description = "what does it do",
                //    Usage = "name /argumentName value",
                //    NeedsAdmin = false,
                //    Opsec = OpsecStatus.NotSet,
                //    Details = "more details about what it does",
                //    Keys = "/argument - what does the key/argument do"
                //},
                new HelpMenuItem()
                {
                    Name = "Add-MachineAccount",
                    Description = "adds a machine account to the domain, can provide optional username and password to authenticate to the domain / other domains",
                    Usage = "Add-MachineAccount /name value /machinePassword value /domain value /username value /password value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.Blocked,
                    Details = "more details about what it does",
                    Keys = "/name - the name of the machine account to create \n/machinePassword - the password to assign the new account \n/domain - the domain to add the machine account to \n/username - the user account to auth with to the target domain \n/password - password for the user account you are authing with"
                },
                new HelpMenuItem()
                {
                    Name = "arp",
                    Description = "executes the built in arp tool to return arp table",
                    Usage = "arp",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.Low,
                    Details = "executes the arp -a command and returns the output",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "cat",
                    Description = "reads the target file ",
                    Usage = "cat /file value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.Moderate,
                    Details = "read a file as a string",
                    Keys = "/file - the location of the file to read , eg. c:\\test.txt"
                },
                new HelpMenuItem()
                {
                    Name = "cd",
                    Description = "changes the current working directory",
                    Usage = "cd /path value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.High,
                    Details = "changes the current working directory to the input path",
                    Keys = "/path - the path to change to"
                },
                new HelpMenuItem()
                {
                    Name = "copy",
                    Description = "copy a file from one location to another",
                    Usage = "copy /file value /dest value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.RequiresLeadAuthorization,
                    Details = "copy source file to destination",
                    Keys = "/file - the source file to copy \n /dest - where you want the file to be copied to"
                },
                new HelpMenuItem()
                {
                    Name = "connect",
                    Description = "starts a tcp server on the current Engineer, or connects into a existing TCP Engineer",
                    Usage = "connect /ip value /port value /localhost value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "can start a tcp server for enginers to connect into or connect into an existing tcp server, to start a server you use the localhost key and the port key, to connect to an existing tcp server you use the ip and port keys",
                    Keys = "/ip - the ip address to connect into for TCP p2p implants \n/port - the port to connect to or listen on for TCP p2p Engineers \n/localhost - true or false, starts the tcp server on the current Engineer if true it will listen only on localhost, if false it will listen on all interfaces"
                },
                new HelpMenuItem()
                {
                    Name = "delete",
                    Description = "removes a file",
                    Usage = "delete /file value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "delete file from system",
                    Keys = "/file - the location of the file to delete"
                },
                new HelpMenuItem()
                { 
                    Name = "download",
                    Description = "downloads the target file to the teamserver",
                    Usage = "download /file value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "downloads the target file to the teamserver",
                    Keys = "/file - the location of the file to download"
                },
                new HelpMenuItem()
                {
                    Name = "execute",
                    Description = "spawns a target process with arguments but no output is returned",
                    Usage = "execute /command value /args value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "spawns a new process with no window using the process .net class and does not redirect output or errors to the C2",
                    Keys = "/command - the program to run, /args - the arguments to pass to the program"
                },
                new HelpMenuItem()
                {
                    Name = "exit",
                    Description = "Exits the program",
                    Usage = "exit",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "Exits the program",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "getLuid",
                    Description = "returns the current luid for the user",
                    Usage = "get_luid",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "get the current user luid",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "getPrivs",
                    Description = "returns the current token privileges",
                    Usage = "getprivs",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "not implement yet",
                    Keys = ""
                },
                 new HelpMenuItem()
                {
                    Name = "GetMachineAccountQuota",
                    Description = "gets the machine account quota for the domain / other domains",
                    Usage = "GetMachineAccountQuota /domain value /username value /password value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "gets the machine account quota for the domain / other domains, this is the number of machine accounts that can be created in the domain, operator can provide an optional domain name, username, and password to other domains",
                    Keys = "/domain - optional domain name to get the machine account quota from \n/username - username to authenticate to the target domain with  \n/password - password for the username to authenticate to the target domain with"
                },
                new HelpMenuItem()
                {
                    Name = "help",
                    Description = "Displays this help menu",
                    Usage = "help /command value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "Displays the help menu it contains all of the commands, the usage, mitre map, and description with the required and optional parameters.",
                    Keys = "/command - the specific command you want help for, if one is not given whole menu is printed.(Optional)"
                },
                new HelpMenuItem()
                {
                    Name = "inject",
                    Description = "injects shellcode of a engineer that matches the selected manager into the target pid",
                    Usage = "inject /manager value /pid value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "reads an engineer from the teamserver that matches the selected manager as shellcode and injects it into the target pid",
                    Keys = "/manager - the name of the maanger to find a matching engineer for, /pid - the pid of the process to inject the shellcode into"
                },
                new HelpMenuItem()
                {
                    Name = "inlineAssembly",
                    Description = "runs the target assembly in memory with the supplied arguments",
                    Usage = "inlineAssembly /file value /args value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "reads the assembly off disk from the teamserver and sends it to the engineer and runs it with the supplied arguments in memory, uses an amsi_patch and etw_patch before running the assembly",
                    Keys = "/file - the location of the assembly to run, /args - the arguments to pass to the assembly"
                },
                new HelpMenuItem()
                {
                    Name = "jump",
                    Description = "lateral movement onto the target machine using a few various techniques",
                    Usage = "jump /method value /target value /manager value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "uses various methods to execute an engineer matching a manager onto the target machine, methods are 1.psexec, 2.winrm, 3.wmi, 4.wmi-ps, 5.dcom",
                    Keys = "/method - the method to use, /target - the target machine to jump to, /manager - the manager to find a matching engineer for"
                },
                 new HelpMenuItem()
                {
                    Name = "ls",
                    Description = "lists the contents of a directory",
                    Usage = "ls /path value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "lists the content in the current directory unless a /path flag is given",
                    Keys = "/path - directory to list (optional)"
                },
                new HelpMenuItem()
                {
                    Name = "make_token",
                    Description = "creates a new token with the provided creds good for remote access",
                    Usage = "make_token /username value /password value /domain value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "creates a token with the given name and password",
                    Keys = "/username - user to make token for \n /password - users password or garbage if using as sacrifical \n /domain - domain the user belongs to"
                },
                new HelpMenuItem()
                {
                    Name = "mkdir",
                    Description = "creates a new directory",
                    Usage = "mkdir /path value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "create a new directory",
                    Keys = "/path - the location of the directory to create"
                },
                new HelpMenuItem()
                {
                    Name = "move",
                    Description = "moves the source file to the destination",
                    Usage = "move /file value /dest value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "moves the target sourse file to the destination location",
                    Keys = "/file - the source file to move, /dest - the destination location"
                },
                new HelpMenuItem()
                {
                    Name = "patch_amsi",
                    Description = "patches amsi in the current process using d/invoke",
                    Usage = "patch_amsi",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "uses d/invoke to patch amsi scan buffer",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "patch_etw",
                    Description = "patches etw in the current process using d/invoke",
                    Usage = "patch_etw",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "patches etw in the current process with d/invoke",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "powershell_import",
                    Description = "imports a powershell script into the engineer to run with powershell, unmanaged powershell",
                    Usage = "powershell_import /import value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "imports a powershell script from the teamserver into the engineer to run with powershell, unmanaged powershell",
                    Keys = "/import - the location of the powershell script to import"
                },
                new HelpMenuItem()
                {
                    Name = "ps",
                    Description = "lists all running processes, the arch, session and owner if possible",
                    Usage = "ps",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "lists al the running processes, the arch, session and owner if possible",
                    Keys = ""
                },
                 new HelpMenuItem()
                {
                    Name = "pwd",
                    Description = "prints the current working directory",
                    Usage = "pwd",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "prints the current working directory",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "rev2sef",
                    Description = "reverts the current token back to the orginal",
                    Usage = "rev2self",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "use after make_token or steal_token to revert the token back to the original",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "run",
                    Description = "executes the target program with the supplied arguments and returns the output to the C2",
                    Usage = "run /command value /args value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "executes the target program/command with the supplied arguments and returns the output to the C2",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "shell",
                    Description = "uses cmd.exe /c to execute the supplied command",
                    Usage = "shell /command value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "runs the suppiled command on cmd.exe",
                    Keys = "/command - the command to run"
                },
                new HelpMenuItem()
                {
                    Name = "shellcodeself",
                    Description = "injects shellcode into the current engineers process and runs it",
                    Usage = "shellcode /program value /args value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "currently broken does not return output to the C2",
                    Keys = "/program - the program to run, /args - the arguments to pass to the program"
                },
                 new HelpMenuItem()
                {
                    Name = "sleep",
                    Description = "sets the sleep time for the engineer",
                    Usage = "sleep /time value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "updates the engineers sleep value to the input value.",
                    Keys = "/time - the time in seconds to sleep"
                },
                new HelpMenuItem()
                {
                    Name = "socks",
                    Description = "starts a socks server on the teamserver at the specified port",
                    Usage = "socks /port value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "starts a socks server on the teamserver at the specified port, forwarding traaffic from the teamserver to the engineer that will forward to the target",
                    Keys = "/port - the port to listen on"
                },
                new HelpMenuItem()
                {
                    Name = "spawn",
                    Description = "starts the spawnto program and injects shellcode into it from an enginner that matches the selected manager",
                    Usage = "spawn /manager value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "runs C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\mscorsvw.exe which is the default spawnto program and injects shellcode into it from an engineer that matches the selected manager",
                    Keys = "/manager - the name of the manager to find a matching engineer for to spawn and connect to"
                },
                new HelpMenuItem()
                {
                    Name = "spawnto",
                    Description = "the target program for spawn",
                    Usage = "spawnto /path value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "the target process for spawn",
                    Keys = "/path - the path to the spawnto program"
                },
                 new HelpMenuItem()
                {
                    Name = "steal_token",
                    Description = "steal a token for a user on the system",
                    Usage = "steal_token /pid value",
                    NeedsAdmin = true,
                    Opsec = OpsecStatus.NotSet,
                    Details = "impersonates a token of a user on the system running the given pid, needs admin to impersonate another logged on users token",
                    Keys = "/pid - the process id of the process to impersonate"
                },
                new HelpMenuItem()
                {
                    Name = "unmanagedPowershell",
                    Description = "uses a powershell runner to execute the supplied command",
                    Usage = "unmanagedpowershell /command value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "has full access to powershell cmdlets and can run commands from powershell import",
                    Keys = "/command - the command to run"
                },
                new HelpMenuItem()
                {
                    Name = "upload",
                    Description = "uploads a file to the target machine",
                    Usage = "upload /file value /dest value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "uploads a file by reading it from the source location on the teamserver and writing it to the destination location",
                    Keys = "/file - the source file to upload, /dest - the destination location"
                },
                new HelpMenuItem()
                {
                    Name = "whoami",
                    Description = "get the current identity of the current user",
                    Usage = "whoami",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "finds who the user is in the local context is not effected by make_token or steal_token",
                    Keys = ""
                }
            };
        }
    }
}
