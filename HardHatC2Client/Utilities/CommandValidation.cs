using HardHatC2Client.Pages;
using static HardHatC2Client.Utilities.CommandKey;

namespace HardHatC2Client.Utilities;

public class CommandItem
{
    public string  Name { get; set; }
    public List<CommandKey>? Keys { get; set; } = new(); //string is key name bool is if its required or not 
}

public class CommandKey
{
    public string Name { get; set; }
    public bool Required { get; set; }
    public List<string>? PreDefinedValues { get; set; } = new List<string>();
    public bool NeedsValues { get; set; }

    public InputType inputType { get; set; }

    public enum InputType
    {
        Text,
        DropDown, //predefined values
        Manager, //useful for commands like inject, jump where u make a new implant based on an existing manager 
        CheckBox, // used for keys that have no values and are either present or not present 
    }

    public CommandKey()
    {
        
    }
    public CommandKey(string name, bool required, InputType inputType, List<string>? preDefinedValues, bool needsValues)
    {
        Name = name;
        Required = required;
        PreDefinedValues = preDefinedValues;
        NeedsValues = needsValues;
        this.inputType = inputType;
    }   
}

public class CommandValidation
{

    public static List<string> ManagerNames
    {
        get { return Managers.managersList.Select(manager => manager.Name).ToList(); }
    }
    
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
            if (arg.StartsWith("/") && commandItem.Keys.Any(x => x.Name.Equals(arg,StringComparison.CurrentCultureIgnoreCase)) )
            {
                argsToParseSplit.Add(arg);
            }
        }


        //foreach required key in the commandItem.Keys dictionary check if the key is in the argsToParseSplit list
        //if it is not then return false
        foreach (var key in commandItem.Keys)
        {
            if (key.Required && !argsToParseSplit.Contains(key.Name))
            {
                args = null;
                error = $"Required argument {key.Name} not found";
                return false;
            }
        }
        //foreach key in the argsToParseSplit list check if the key is in the commandItem.Keys dictionary and that it has not already been added to the args dictionary if it has not then add it to the args dictionary
        foreach (string arg in argsToParseSplit)
        {
            if (commandItem.Keys.Any(x=> x.Name ==arg) && !args.ContainsKey(arg))
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
            Keys = new List<CommandKey>()
                    { 
                        new CommandKey() {Name = "/username", Required = false, inputType = InputType.Text, PreDefinedValues = null, NeedsValues = true},
                        new CommandKey("/password",false,InputType.Text, null, true),
                        new CommandKey("/domain",false,InputType.Text, null, true),
                        new CommandKey("/name",true,InputType.Text, null,  true),
                        new CommandKey("/machinePassword",true,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "arp",
            Keys = null //no keys
        },
        new CommandItem()
        {
            Name = "cat",
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file",true,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "cd",
            //Keys = {{"/path",true},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/path",true,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "copy",
           // Keys = {{"/file",true},{"/dest",true},}
           Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file",true,InputType.Text, null,  true),
                        new CommandKey("/dest",true,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "connect",
            //Keys = {{"/ip",false},{"/port",true},{"/localhost",false},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/ip",false,InputType.Text, null,  true),
                        new CommandKey("/port",true,InputType.Text, null,  true),
                        new CommandKey("/localhost",false,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "delete",
            //Keys = {{"/file",true}}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file",true,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "download",
           // Keys = {{"/file",true}}
           Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file",true,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "execute",
            //Keys = {{"/command",true},{"/args",false},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/command",true,InputType.Text, null,  true),
                        new CommandKey("/args",false,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "executeAssembly",
           // Keys = {{"/file",true},{"/args",false},}
           Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file",true,InputType.Text, null,  true),
                        new CommandKey("/args",false,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "exit",
            Keys = null
        },
        new CommandItem()
        {
            Name = "getluid",
            Keys = null
        },
        new CommandItem()
        {
            Name = "getprivs",
            Keys = null
        },
        new CommandItem()
        {
            Name = "GetMachineAccountQuota",
            //Keys = {{"/domain",false},{"/username",false},{"/password",false},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/domain",false,InputType.Text, null,  true),
                        new CommandKey("/username",false,InputType.Text, null,  true),
                        new CommandKey("/password",false,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "help",
            //Keys = {{"/command",false}}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/command",false,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "inject",
            //Keys = {{"/manager",true},{"/pid",true},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/manager",true,InputType.Manager, ManagerNames,  true),
                        new CommandKey("/pid",true,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "inlineAssembly",
            //Keys = {{"/file",true},{"/args",false}, {"/execmethod",false }, {"/appdomain",false },}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file",true,InputType.Text, null,  true),
                        new CommandKey("/args",false,InputType.Text, null,  true),
                        new CommandKey("/execmethod",false,InputType.DropDown, new List<string>(){"UnloadDomain","Classic"},  true),
                        new CommandKey("/appdomain",false,InputType.Text, null,  true),
                        new CommandKey("/Patch_A",false,InputType.CheckBox,null,false)
                    }
        },
        new CommandItem()
        {
            Name = "inlineDll",
            //Keys = {{"/dll",true},{"/function",true},{"/args",false}}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/dll",true,InputType.Text, null,  true),
                        new CommandKey("/function",true,InputType.Text, null,  true),
                        new CommandKey("/args",false,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "ipconfig",
            Keys = null
        },
        new CommandItem()
        {
            Name = "jump",
            //Keys = {{"/method",true},{"/target",true},{"/manager",true}}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/method",true,InputType.DropDown, new List<string>(){"psexec","wmi","wmi-ps","winrm","dcom"},  true),
                        new CommandKey("/target",true,InputType.Text, null,  true),
                        new CommandKey("/manager",true,InputType.Manager, ManagerNames,  true)
                    }
        },
        new CommandItem()
        {
            Name = "ldapSearch",
            //Keys = {{"/search",true},{"/domain",false},{"/username",false},{"/password",false},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/search",true,InputType.Text, null,  true),
                        new CommandKey("/domain",false,InputType.Text, null,  true),
                        new CommandKey("/username",false,InputType.Text, null,  true),
                        new CommandKey("/password",false,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "link",
            //Keys = {{"/pipe",true},{"/ip",false},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/pipe",true,InputType.Text, null,  true),
                        new CommandKey("/ip",false,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "loadAssembly",
           // Keys = {{"/file",true},{"/args",false},}
              Keys = new List<CommandKey>()
                      {
                            new CommandKey("/file",true,InputType.Text, null,  true),
                            new CommandKey("/args",false,InputType.Text, null,  true)
                      }
        },
        new CommandItem()
        {
            Name = "ls",
            //Keys = {{"/path",false},{"/getcount",false},{"/getacls",false},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/path",false,InputType.Text, null,  true),
                        new CommandKey("/getcount",false,InputType.Text, null,  true),
                        new CommandKey("/getacls",false,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "make_token",
            //Keys = {{"/username",true},{"/password",true},{"/domain",true},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/username",true,InputType.Text, null,  true),
                        new CommandKey("/password",true,InputType.Text, null,  true),
                        new CommandKey("/domain",true,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "mkdir",
            //Keys = {{"/path",true},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/path",true,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "move",
            //Keys = {{"/file",true},{"/dest",true},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file",true,InputType.Text, null,  true),
                        new CommandKey("/dest",true,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "net-localgroup",
            Keys = null
        },
        new CommandItem()
        {
            Name = "net-localgroup-members",
            //Keys = {{"/group",false},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/group",false,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "net-Dclist",
            //Keys = {{"/domain",false},{"/username",false},{"/password",false},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/domain",false,InputType.Text, null,  true),
                        new CommandKey("/username",false,InputType.Text, null,  true),
                        new CommandKey("/password",false,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "patch_amsi",
            Keys = null
        },
        new CommandItem()
        {
            Name = "patch_etw",
            Keys = null
        },
        new CommandItem()
        {
            Name = "powershell_import",
            //Keys = {{"/import",true},{"/remove",false},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/import",true,InputType.Text, null,  true),
                        new CommandKey("/remove",false,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "powerlist",
            Keys = null
        },
        new CommandItem()
        {
            Name = "ps",
            Keys = null
        },
        new CommandItem()
        {
            Name = "print-env",
            Keys = null
        },
        new CommandItem()
        {
            Name = "pwd",
            Keys = null
        },
        new CommandItem()
        {
            Name = "rev2self",
            Keys = null
        },
        new CommandItem()
        {
            Name = "rportforward",
            //Keys = {{"/fwdport",true},{"/fwdhost",true},{"/bindport",true},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/fwdport",true,InputType.Text, null,  true),
                        new CommandKey("/fwdhost",true,InputType.Text, null,  true),
                        new CommandKey("/bindport",true,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "run",
            //Keys = {{"/command",true},{"/args",false}, }
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/command",true,InputType.Text, null,  true),
                        new CommandKey("/args",false,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "shell",
            //Keys = {{"/command",true},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/command",true,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "InlineShellcode",
            //Keys = {{"/program",true},{"/args",false},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/program",true,InputType.Text, null,  true),
                        new CommandKey("/args",false,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "sleep",
            //Keys = {{"/time",true},}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/time",true,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "socks",
            //Keys = {{"/port",true}}
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/port",true,InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "spawn",
           // Keys = {{"/manager",true}}
            Keys = new List<CommandKey>()
            {
                new CommandKey("/manager",true,InputType.Manager, ManagerNames,  true)
            }
        },
        new CommandItem()
        {
            Name = "spawnto",
            //Keys = {{"/path",true}}
            Keys = new List<CommandKey>()
            {
                new CommandKey("/path",true,InputType.Text, null,  true)
            }
        },
        new CommandItem()
        {
            Name = "steal_token",
           // Keys = {{"/pid",true}}
            Keys = new List<CommandKey>()
            {
                new CommandKey("/pid",true,InputType.Text, null,  true)
            }
        },
        new CommandItem()
        {
            Name = "unmanagedPowershell",
            //Keys = {{"/command",true}}
            Keys = new List<CommandKey>()
            {
                new CommandKey("/command",true,InputType.Text, null,  true)
            }
        },
        new CommandItem()
        {
            Name = "upload",
           // Keys = {{"/file",true},{"/dest",true},}
            Keys = new List<CommandKey>()
            {
                new CommandKey("/file",true,InputType.Text, null,  true),
                new CommandKey("/dest",true,InputType.Text, null,  true)
            }
        },
        new CommandItem()
        {
            Name = "whoami",
            //Keys = {{"/groups",false},}
            Keys = new List<CommandKey>()
            {
                new CommandKey("/groups",false,InputType.CheckBox, null,  false)
            }
        },
    };
}