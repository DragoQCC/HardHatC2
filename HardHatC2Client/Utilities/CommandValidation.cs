namespace HardHatC2Client.Utilities;

public class CommandItem
{
    public string  Name { get; set; }
    public Dictionary<string,bool> Keys { get; set; } = new  Dictionary<string,bool>(); //string is key name bool is if its required or not 
}

public class CommandValidation
{

    public static bool ValidateCommand(string input, out Dictionary<string,string> args , out string error)
    {
        args = new Dictionary<string, string>();
        string command = input.Split(' ')[0];
        List<string> argsToParse = input.Split(' ').Skip(1).ToList();

        //if none of the command names in CommandList match the command name then return false
        if (!CommandList.Any(x => x.Name.Equals(command, StringComparison.OrdinalIgnoreCase)))
        {
            args = null;
            error = "Command not found";
            return false;
        }
        
        //get the command item from the CommandList that matches the command name
        CommandItem commandItem = CommandList.First(x => x.Name.Equals(command, StringComparison.OrdinalIgnoreCase));
        
        //if the commandItem has no keys then return true
        if (commandItem.Keys.Count == 0)
        {
            error = null;
            return true;
        }


        List<string> argsToParseSplit = new List<string>();
        
        argsToParse = argsToParse.Select(x => x.Trim()).ToList();
        //remove any empty strings from the argsToParse list
        argsToParse = argsToParse.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        
        //take the argsToParse list if the string starts with a / and add it to the argsToParseSplit list
        foreach (var arg in argsToParse)
        {
            if (arg.StartsWith("/") && commandItem.Keys.ContainsKey(arg))
            {
                argsToParseSplit.Add(arg);
            }
        }


        //foreach required key in the commandItem.Keys dictionary check if the key is in the argsToParseSplit list
        //if it is not then return false
        foreach (var key in commandItem.Keys)
        {
            if (key.Value && !argsToParseSplit.Contains(key.Key))
            {
                args = null;
                error = $"Required argument {key.Key} not found";
                return false;
            }
        }
        //foreach key in the argsToParseSplit list check if the key is in the commandItem.Keys dictionary and that it has not already been added to the args dictionary if it has not then add it to the args dictionary
        foreach (string arg in argsToParseSplit)
        {
            if (commandItem.Keys.ContainsKey(arg) && !args.ContainsKey(arg))
            {
                string value = "";
                //take the argsToParse list find the entry that matches the current arg and get entries after that until the next entry is another value in the argsToParseSplit list
                //then join the entries together with a space and add it to the args dictionary
                value = string.Join(" ", argsToParse.SkipWhile(x => x != arg).Skip(1).TakeWhile(x => !argsToParseSplit.Contains(x)));
                args.Add(arg,value);
            }
        }
        error = null;
        return true;
    }

    public static List<CommandItem> CommandList = new List<CommandItem>()
    {
        // new CommandItem()
        // {
        //     Name = "",
        //     Keys = {{"/",false},{"/",false},}
        // },
        new CommandItem()
        {
            Name = "Add-MachineAccount",
            Keys = {{"/username",false},{"/password",false},{"/domain",false},{"/name",true},{"/machinePassword",true}}
        },
        new CommandItem()
        {
            Name = "arp",
            Keys = {}
        },
        new CommandItem()
        {
            Name = "cat",
            Keys = {{"/file",true},}
        },
        new CommandItem()
        {
            Name = "cd",
            Keys = {{"/path",true},}
        },
        new CommandItem()
        {
            Name = "copy",
            Keys = {{"/file",true},{"/dest",true},}
        },
        new CommandItem()
        {
            Name = "connect",
            Keys = {{"/ip",false},{"/port",true},{"/localhost",false},}
        },
        new CommandItem()
        {
            Name = "delete",
            Keys = {{"/file",true}}
        },
        new CommandItem()
        {
            Name = "download",
            Keys = {{"/file",true}}
        },
        new CommandItem()
        {
            Name = "execute",
            Keys = {{"/command",true},{"/args",false},}
        },
        new CommandItem()
        {
            Name = "executeAssembly",
            Keys = {{"/file",true},{"/args",false},}
        },
        new CommandItem()
        {
            Name = "exit",
            Keys = {}
        },
        new CommandItem()
        {
            Name = "getluid",
            Keys = {}
        },
        new CommandItem()
        {
            Name = "getprivs",
            Keys = {}
        },
        new CommandItem()
        {
            Name = "GetMachineAccountQuota",
            Keys = {{"/domain",false},{"/username",false},{"/password",false},}
        },
        new CommandItem()
        {
            Name = "help",
            Keys = {{"/command",false}}
        },
        new CommandItem()
        {
            Name = "inject",
            Keys = {{"/manager",true},{"/pid",true},}
        },
        new CommandItem()
        {
            Name = "inlineAssembly",
            Keys = {{"/file",true},{"/args",false}, {"/execmethod",false }, {"/appdomain",false },}
        },
        new CommandItem()
        {
            Name = "inlineDll",
            Keys = {{"/dll",true},{"/function",true},{"/args",false}}
        },
        new CommandItem()
        {
            Name = "ipconfig",
            Keys = {}
        },
        new CommandItem()
        {
            Name = "jump",
            Keys = {{"/method",true},{"/target",true},{"/manager",true}}
        },
        new CommandItem()
        {
            Name = "ldapSearch",
            Keys = {{"/search",true},{"/domain",false},{"/username",false},{"/password",false},}
        },
        new CommandItem()
        {
            Name = "link",
            Keys = {{"/pipe",true},{"/ip",false},}
        },
        new CommandItem()
        {
            Name = "loadAssembly",
            Keys = {{"/file",true},{"/args",false},}
        },
        new CommandItem()
        {
            Name = "ls",
            Keys = {{"/path",false},{"/getcount",false},{"/getacls",false},}
        },
        new CommandItem()
        {
            Name = "make_token",
            Keys = {{"/username",true},{"/password",true},{"/domain",true},}
        },
        new CommandItem()
        {
            Name = "mkdir",
            Keys = {{"/path",true},}
        },
        new CommandItem()
        {
            Name = "move",
            Keys = {{"/file",true},{"/dest",true},}
        },
        new CommandItem()
        {
            Name = "net-localgroup",
            Keys = {}
        },
        new CommandItem()
        {
            Name = "net-localgroup-members",
            Keys = {{"/group",false},}
        },
        new CommandItem()
        {
            Name = "net-Dclist",
            Keys = {{"/domain",false},{"/username",false},{"/password",false},}
        },
        new CommandItem()
        {
            Name = "patch_amsi",
            Keys = {}
        },
        new CommandItem()
        {
            Name = "patch_etw",
            Keys = {}
        },
        new CommandItem()
        {
            Name = "powershell_import",
            Keys = {{"/import",true},{"/remove",false},}
        },
        new CommandItem()
        {
            Name = "powerlist",
            Keys = {}
        },
        new CommandItem()
        {
            Name = "ps",
            Keys = {}
        },
        new CommandItem()
        {
            Name = "print-env",
            Keys = {}
        },
        new CommandItem()
        {
            Name = "pwd",
            Keys = {}
        },
        new CommandItem()
        {
            Name = "rev2self",
            Keys = {}
        },
        new CommandItem()
        {
            Name = "rportforward",
            Keys = {{"/fwdport",true},{"/fwdhost",true},{"/bindport",true},}
        },
        new CommandItem()
        {
            Name = "run",
            Keys = {{"/command",true},{"/args",false}, }
        },
        new CommandItem()
        {
            Name = "shell",
            Keys = {{"/command",true},}
        },
        new CommandItem()
        {
            Name = "InlineShellcode",
            Keys = {{"/program",true},{"/args",false},}
        },
        new CommandItem()
        {
            Name = "sleep",
            Keys = {{"/time",true},}
        },
        new CommandItem()
        {
            Name = "socks",
            Keys = {{"/port",true}}
        },
        new CommandItem()
        {
            Name = "spawn",
            Keys = {{"/manager",true}}
        },
        new CommandItem()
        {
            Name = "spawnto",
            Keys = {{"/path",true}}
        },
        new CommandItem()
        {
            Name = "steal_token",
            Keys = {{"/pid",true}}
        },
        new CommandItem()
        {
            Name = "unmanagedPowershell",
            Keys = {{"/command",true}}
        },
        new CommandItem()
        {
            Name = "upload",
            Keys = {{"/file",true},{"/dest",true},}
        },
        new CommandItem()
        {
            Name = "whoami",
            Keys = {{"/groups",false},}
        },
    };
}