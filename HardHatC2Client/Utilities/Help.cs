namespace HardHatC2Client.Utilities
{
    public class Help
    {
       public static List<HelpMenuItem> menuItems = HelpMenuItem.itemList;        
        public static List<HelpMenuItem> DisplayHelp(Dictionary<string,string> input)
        {
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
            
            public string MitreTechnique { get; set; }

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
                    Name = "addcommand",
                    Description = "takes a .cs file from the engineer/Commands , compiles it, and then sends it to the engineer to be loaded as new commmand.",
                    Usage = "addcommand /command value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "takes a .cs file from the engineer/Commands  , compiles it, and then sends it to the engineer to be loaded as new commmand.",
                    Keys = "/command - name of the c# file to compile and send to the engineer"
                },
                 new HelpMenuItem()
                {
                    Name = "addmodule",
                    Description = "takes a .cs file from the engineer where the class has the Module Attribute , compiles it, and then sends it to the engineer to be loaded as new module.",
                    Usage = "addmodule /module value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    Details = "takes a .cs file from the engineer where the class has the Module Attribute, compiles it, and then sends it to the engineer to be loaded as new module.",
                    Keys = "/module - name of the c# file to compile and send to the engineer"
                },
                new HelpMenuItem()
                {
                    Name = "Add-MachineAccount",
                    Description = "adds a machine account to the domain, can provide optional username and password to authenticate to the domain / other domains",
                    Usage = "Add-MachineAccount /name value /machinePassword value /domain value /username value /password value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1136",
                    Details = "more details about what it does",
                    Keys = "/name - the name of the machine account to create \n/machinePassword - the password to assign the new account \n/domain - the domain to add the machine account to \n/username - the user account to auth with to the target domain \n/password - password for the user account you are authing with"
                },
                new HelpMenuItem()
                {
                    Name = "arp",
                    Description = "executes the built in arp tool to return arp table",
                    Usage = "arp",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1049",
                    Details = "executes the arp -a command and returns the output",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "cat",
                    Description = "reads the target file ",
                    Usage = "cat /file value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1083",
                    Details = "read a file as a string",
                    Keys = "/file - the location of the file to read , eg. c:\\test.txt"
                },
                new HelpMenuItem()
                {
                    Name = "cd",
                    Description = "changes the current working directory",
                    Usage = "cd /path value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1083",
                    Details = "changes the current working directory to the input path",
                    Keys = "/path - the path to change to"
                },
                // new HelpMenuItem()
                //{
                //    Name = "cleanupinteractiveprofile",
                //    Description = "removes a profile from the local computer, intended user is for after make_token with the /local flag with uses an interactive logon which makes a profile if one did not exist before",
                //    Usage = "cleanupinteractiveprofile /sid value",
                //    NeedsAdmin = false,
                //    Opsec = OpsecStatus.NotSet,
                //    MitreTechnique = "T1083",
                //    Details = "removes a profile from the local computer, intended user is for after make_token with the /local flag with uses an interactive logon which makes a profile if one did not exist before, performs a revert to self to profile can be deleted.",
                //    Keys = "/sid - SID of user to delete profile for"
                //},
                new HelpMenuItem()
                {
                    Name = "copy",
                    Description = "copy a file from one location to another",
                    Usage = "copy /file value /dest value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1105",
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
                    MitreTechnique = "T1095",
                    Details = "can start a tcp server for enginers to connect into or connect into an existing tcp server, to start a server you use the localhost key and the port key, to connect to an existing tcp server you use the ip and port keys",
                    Keys = "/ip - the ip address to connect into for TCP p2p implants \n/port - the port to connect to or listen on for TCP p2p Engineers \n/localhost - true or false, starts the tcp server on the current Engineer if true it will listen only on localhost, if false it will listen on all interfaces"
                },
                //new HelpMenuItem()
                //{
                //    Name = "createprocess_stolentoken",
                //    Description = "creates a process with a stolen token from the token store",
                //    Usage = "createprocess_stolentoken /program value /args value /index value",
                //    NeedsAdmin = false,
                //    Opsec = OpsecStatus.NotSet,
                //    MitreTechnique = "T1134",
                //    Details = "creates a process with a stolen token from the token store",
                //    Keys = "/program - the program to execute \n/args - the arguments to pass to the program \n/index - the index of the token to steal from the token store"
                //},
                new HelpMenuItem()
                {
                    Name = "delete",
                    Description = "removes a file",
                    Usage = "delete /file value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1070",
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
                    MitreTechnique = "T1105",
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
                    MitreTechnique = "T1059",
                    Details = "spawns a new process with no window using the process .net class and does not redirect output or errors to the C2",
                    Keys = "/command - the program to run, /args - the arguments to pass to the program"
                },
                new HelpMenuItem()
                {
                    Name = "executeAssembly",
                    Description = "executes the provided assembly in memory of the spawn to process, it is injected as shellcode and then executed",
                    Usage = "executeAssembly /file value /args value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1055",
                    Details = "by default will create the spawnto process, then injects the target assembly as shellcode made from donut and then executes the shellcode, the output is read over a named pipe and returned to the C2",
                    Keys = "/file - location on teamserver to the assembly to execute \n/args - the arguments to pass to the assembly"
                },
                new HelpMenuItem()
                {
                    Name = "execute_bof",
                    Description = "executes the provided COFF/BOF file inline on the engineer",
                    Usage = "execute_bof /file value /function value /argtypes value /args value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1055",
                    Details = "executes a bof/coff file on engineer",
                    Keys = "/file - location on teamserver to the bof to execute in memory \n/function - the entrypoint function to call on the bof \n/argtypes - the type for the arguments in order based on normal bof convention (b,i,s,z,Z) \n/args - the arguments to pass to the bof"
                },
                new HelpMenuItem()
                {
                    Name = "execute_pe",
                    Description = "executes the provided PE file inline on the engineer",
                    Usage = "execute_pe /file value /args value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1055",
                    Details = "executes a bof/coff file on engineer",
                    Keys = "/file - location on teamserver to the executable to execute in memory \n/args - the arguments to pass to the bof"
                },
                new HelpMenuItem()
                {
                    Name = "exit",
                    Description = "Exits the program",
                    Usage = "exit",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "",
                    Details = "Exits the program",
                    Keys = ""
                },
                 new HelpMenuItem()
                {
                    Name = "GetCommands",
                    Description = "returns the current engineers loaded commands",
                    Usage = "GetCommands",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "",
                    Details = "returns the current engineers loaded commands, new commands can be added with AddCommand",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "get_luid",
                    Description = "returns the current luid for the user",
                    Usage = "get_luid",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.High,
                    MitreTechnique = "",
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
                    MitreTechnique = "",
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
                    MitreTechnique = "",
                    Details = "gets the machine account quota for the domain / other domains, this is the number of machine accounts that can be created in the domain, operator can provide an optional domain name, username, and password to other domains",
                    Keys = "/domain - optional domain name to get the machine account quota from \n/username - username to authenticate to the target domain with  \n/password - password for the username to authenticate to the target domain with"
                },
                 new HelpMenuItem()
                 {
                     Name = "getSystem",
                     Description = "Elevates current process to SYSTEM or executes command as SYSTEM",
                     Usage = "getSystem /elevate /command value",
                     NeedsAdmin = true,
                     Opsec = OpsecStatus.NotSet,
                     MitreTechnique = "T1134",
                     Details = "Elevates current process to SYSTEM or executes command as SYSTEM",
                     Keys = "/elevate - elevates the current process to SYSTEM, /command - executes the command as SYSTEM"
                 },
                new HelpMenuItem()
                {
                    Name = "help",
                    Description = "Displays this help menu",
                    Usage = "help /command value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "",
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
                    MitreTechnique = "T1055",
                    Details = "reads an engineer from the teamserver that matches the selected manager as shellcode and injects it into the target pid",
                    Keys = "/manager - the name of the maanger to find a matching engineer for, /pid - the pid of the process to inject the shellcode into"
                },
                new HelpMenuItem()
                {
                    Name = "inlineAssembly",
                    Description = "runs the target assembly in memory with the supplied arguments",
                    Usage = "inlineAssembly /file value /args value /execmethod OptionalValue /appdomain OptionalValue",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "",
                    Details = "reads the assembly off disk from the teamserver and sends it to the engineer and runs it with the supplied arguments in memory, uses an amsi_patch and etw_patch before running the assembly",
                    Keys = "/file - the location of the assembly to run \n /args - the arguments to pass to the assembly \n /execmethod - valid options are UnloadDomain or classic \n (UnloadDomain creates a new app domain then unloads it, can cause issues, classic just loads the assembly in the current appdomain but that means it can be seen later) \n /appdomain the name of the appdomain default is mscorlib"
                },
                new HelpMenuItem()
                {
                    Name = "inlineDll",
                    Description = "runs the target dll in memory with the supplied arguments",
                    Usage = "inlineDll /dll value /function value /args value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1055",
                    Details = "reads the dll off disk from the teamserver and sends it to the engineer and runs it with the supplied arguments in memory, attempts to perform module overload to hide in a legit dll",
                    Keys = "/dll - the location of the dll on the ts to execute \n/function the exportyed dll function to invoke \n/args - the arguments to pass to the function"
                },
                new HelpMenuItem()
                {
                    Name = "ipconfig",
                    Description = "gets a list of all ip addresses & masks on the target machine",
                    Usage = "ipconfig",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "",
                    Details = "gets a list of all ip addresses & masks on the target machine",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "jump",
                    Description = "lateral movement onto the target machine using a few various techniques",
                    Usage = "jump /method value /target value /manager value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1021",
                    Details = "uses various methods to execute an engineer matching a manager onto the target machine, methods are 1.psexec, 2.winrm, 3.wmi, 4.wmi-ps, 5.dcom",
                    Keys = "/method - the method to use, /target - the target machine to jump to, /manager - the manager to find a matching engineer for"
                },
                new HelpMenuItem()
                {
                    Name = "ldapSearch",
                    Description = "performs an ldap search on the current domain or provided domain",
                    Usage = "ldapSearch /search value /domain value /username value /password value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1087",
                    Details = "connects using ldap to the target domain and performs a search for the provided search term, operator can provide an optional domain name, username, and password to other domains",
                    Keys = "/search - the search term to use \n/domain - optional domain name to search in \n/username - username to authenticate to the target domain with  \n/password - password for the username to authenticate to the target domain with"
                },
                new HelpMenuItem()
                {
                    Name = "link",
                    Description = "",
                    Usage = "link /pipe value /ip optionalValue",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1570,T1572",
                    Details = "",
                    Keys = "/pipe - the name of the pipe to connect to or create \n /ip the ip address to connect to in a reverse connection(optional)" 
                },
                new HelpMenuItem()
                {
                    Name = "loadAssembly",
                    Description = "gets the provided assembly from the teamserver and loads it into the current process, uses D/Invoke to map to memory, and then invoke the main function EXPERIMENTAL",
                    Usage = "loadAssembly /file value /args value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1055",
                    Details = "gets the provided assembly from the teamserver and loads it into the current process, uses D/Invoke to map to memory, and then invoke the main function EXPERIMENTAL",
                    Keys = "/file - the location of the assembly on the ts to load \n/args - the arguments to pass to the assembly"
                },
                new HelpMenuItem()
                {
                    Name = "ls",
                    Description = "lists the contents of a directory",
                    Usage = "ls /path optionalValue /getcount optionalValue /getacls optionalValue ",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1083",
                    Details = "lists the content in the current directory unless a /path flag is given",
                    Keys = "/path - directory to list (optional), /getcount - get the number of items in the subdirectory (optional), /getacls - get the acls of the items in the directory (optional)"
                },
                new HelpMenuItem()
                {
                    Name = "make_token",
                    Description = "creates a new token with the provided creds good for remote access",
                    Usage = "make_token /username value /password value /domain value", ///localauth
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1134",
                    Details = "creates a token with the given name and password",
                    Keys = "/username - user to make token for \n /password - users password or garbage if using as sacrifical \n /domain - domain the user belongs to \n" ///localauth - if enabled uses interactive logon type to enable local and remote access
                },
                new HelpMenuItem()
                {
                    Name = "mimikatz",
                    Description = "injects mimikatz dll into the local process using d/invoke which will perform module overload to try and hide it.",
                    Usage = "mimikatz /args Value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "",
                    Details = "uses the inline dll command to run the powerkatz dll in memory and then runs the provided arguments",
                    Keys = "/args - the arguments to pass to mimikatz"
                },
                new HelpMenuItem()
                {
                    Name = "mkdir",
                    Description = "creates a new directory",
                    Usage = "mkdir /path value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1083",
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
                    MitreTechnique = "T1083",
                    Details = "moves the target sourse file to the destination location",
                    Keys = "/file - the source file to move, /dest - the destination location"
                },
                new HelpMenuItem()
                {
                    Name = "net-localgroup",
                    Description = "returns a list of all local groups on the current machine",
                    Usage = "net-localgroup",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1087",
                    Details = "returns a list of all local groups on the current machine",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "net-localgroup-members",
                    Description = "returns a list of all members of the specified local group on the current machine, if no group is specified it will return all members of all local groups",
                    Usage = "net-localgroup-members /group value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1087",
                    Details = "returns a list of all members of the specified local group on the current machine, if no group is specified it will return all members of all local groups",
                    Keys = "/group - the local group to get members of (optional)"
                },
                new HelpMenuItem()
                {
                    Name = "net-Dclist",
                    Description = "returns a list of all domain controllers on the current domain, or the domain specified",
                    Usage = "net-Dclist /domain value /username value /password value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1087",
                    Details = "returns a list of all domain controllers on the current domain, or the domain specified",
                    Keys = "/domain - the domain to get domain controllers for (optional) \n /username - username to authenticate to the target domain with (optional) \n /password - password for the username to authenticate to the target domain with (optional)"
                },
                new HelpMenuItem()
                {
                    Name = "patch_amsi",
                    Description = "patches amsi in the current process using d/invoke",
                    Usage = "patch_amsi",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1562",
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
                    MitreTechnique = "T1562",
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
                    MitreTechnique = "T1059",
                    Details = "imports a powershell script from the teamserver into the engineer to run with powershell, unmanaged powershell",
                    Keys = "/import - the location of the powershell script to import,\n /remove - the number of the script to remove"
                },
                new HelpMenuItem()
                {
                    Name = "powerlist",
                    Description = "lists the currently imported powershell scripts",
                    Usage = "powerlist",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1059",
                    Details = "lists the currently imported powershell scripts,all are loaded when unmanaged powershell is run",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "ps",
                    Description = "lists all running processes, the arch, session and owner if possible",
                    Usage = "ps",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1057",
                    Details = "lists al the running processes, the arch, session and owner if possible",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "print-env",
                    Description = "lists all the environment variables",
                    Usage = "print-env",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1082",
                    Details = "lists al the environment variables",
                    Keys = ""
                },
                 new HelpMenuItem()
                {
                    Name = "pwd",
                    Description = "prints the current working directory",
                    Usage = "pwd",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1083",
                    Details = "prints the current working directory",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "rev2self",
                    Description = "reverts the current token back to the orginal",
                    Usage = "rev2self",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1134",
                    Details = "use after make_token or steal_token to revert the token back to the original",
                    Keys = ""
                },
                new HelpMenuItem()
                {
                    Name = "rportForward",
                    Description = "sets a reverse port forward from the implant to the teamserver, which then sends the data to the specified host and port",
                    Usage = "rportForward /fwdhost value /fwdport value /bindport value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1571",
                    Details = "sets a reverse port forward from the implant to the teamserver, which then sends the data to the specified host and port ",
                    Keys = "/fwdhost - the host to forward the data to \n /fwdport - the port to forward the data to \n /bindport - the port to bind the reverse port forward on the implant to"
                },
                new HelpMenuItem()
                {
                    Name = "run",
                    Description = "executes the target program with the supplied arguments and returns the output to the C2",
                    Usage = "run /command value /args value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1059",
                    Details = "executes the target program/command with the supplied arguments and returns the output to the C2",
                    Keys = "/command - the program to spawn \n /args - the arguments to pass the program"
                },
                new HelpMenuItem()
                {
                    Name = "runas",
                    Description = "executes the target program with the supplied arguments and returns the output to the C2, using the supplied credentials",
                    Usage = "runas /program value /args value /username value /password value /domain value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1059",
                    Details = "executes the target program/command with the supplied arguments and returns the output to the C2, using the supplied credentials, if using local creds set the domain to a .",
                    Keys = "/program - the program to spawn \n /args - the arguments to pass the program \n /username - the username to use \n /password - the password to use \n /domain - the domain to use"
                },
                new HelpMenuItem()
                {
                    Name = "shell",
                    Description = "uses cmd.exe /c to execute the supplied command",
                    Usage = "shell /command value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1059",
                    Details = "runs the suppiled command on cmd.exe",
                    Keys = "/command - the command to run"
                },
                new HelpMenuItem()
                {
                    Name = "InlineShellcode",
                    Description = "injects shellcode into the current engineers process and runs it",
                    Usage = "InlineShellcode /program value /args value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1055",
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
                    MitreTechnique = "T1029",
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
                    MitreTechnique = "T1571",
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
                    MitreTechnique = "T1055",
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
                    MitreTechnique = "",
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
                    MitreTechnique = "T1134",
                    Details = "impersonates a token of a user on the system running the given pid, needs admin to impersonate another logged on users token",
                    Keys = "/pid - the process id of the process to impersonate"
                },
                 new HelpMenuItem()
                 { 
                     Name = "token_store",
                     Description = "stores a token for later use",
                     Usage = "token_store /view /use value",
                     NeedsAdmin = false,
                     Opsec = OpsecStatus.NotSet,
                     MitreTechnique = "T1134",
                     Details = "impersonates a token of a user on the system running the given pid, needs admin to impersonate another logged on users token",
                    Keys = "/view - loads the token store table \n/use - loads a token to use based off its index"
                 },
                new HelpMenuItem()
                {
                    Name = "unmanagedPowershell",
                    Description = "uses a powershell runner to execute the supplied command",
                    Usage = "unmanagedpowershell /command value",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1059",
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
                    MitreTechnique = "T1105",
                    Details = "uploads a file by reading it from the source location on the teamserver and writing it to the destination location",
                    Keys = "/file - the source file to upload, /dest - the destination location"
                },
                new HelpMenuItem()
                {
                    Name = "whoami",
                    Description = "get the current identity of the current user",
                    Usage = "whoami /groups OptionalValue",
                    NeedsAdmin = false,
                    Opsec = OpsecStatus.NotSet,
                    MitreTechnique = "T1033",
                    Details = "finds who the user is in the local context is not effected by make_token or steal_token",
                    Keys = "/groups - Optional, if set to true will return the groups the user is a member of, if false or not present then will not get group info"
                }
            };
        }
    }
}
