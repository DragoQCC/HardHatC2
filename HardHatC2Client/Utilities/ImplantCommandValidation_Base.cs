﻿using HardHatCore.HardHatC2Client.Pages;
using HardHatCore.HardHatC2Client.Plugin_Interfaces;

namespace HardHatCore.HardHatC2Client.Utilities;

public class CommandItem
{
    public string  Name { get; set; }
    public string Description { get; set; }
    public string Usage { get; set; }
    public bool NeedsAdmin { get; set; }
    public OpsecStatus Opsec { get; set; }
    public string MitreTechnique { get; set; }
    public bool RequiresPreProc { get; set; }
    public bool RequiresPostProc { get; set; }
    public List<CommandKey>? Keys { get; set; } = new(); //string is key name bool is if its required or not 

    public enum OpsecStatus
    {
        NotSet,
        Low,
        Moderate,
        High,
        RequiresLeadAuthorization,
        Blocked
    }
}

public class CommandKey
{
    public string Name { get; set; }
    public string Description { get; set; }
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
        File, // used for keys that require a file to be uploaded
    }

    public CommandKey()
    { }
    public CommandKey(string name,string desc, bool required, InputType inputType, List<string>? preDefinedValues, bool needsValues)
    {
        Name = name;
        Description = desc;
        Required = required;
        PreDefinedValues = preDefinedValues;
        NeedsValues = needsValues;
        this.inputType = inputType;
    }   
}


public class ImplantCommandValidation_Base : IImplantCommandValidation
{
    //name of the plugin, needs to be unique
    public string Name { get; set; } = "Default";

    public I_ImplantCommandValidationBaseData _metadata { get; set; } = new ImplantCommandValidationBaseData()
    { 
        Name = "Default",
        Description = "This is the built in Implant Task Verification, override this if you need custom options when verifying tasks"
    };

    public virtual List<string> GetRequiredCommandList()
    {
        return new List<string>() { "Addcommand", "AddModule", "connect", "CheckIn", "link", "FirstCheckIn", "exit", "socks", "rportforward", "canceltask", "GetCommands", "UpdateTaskKey" };
    }

    public virtual List<string> GetOptionalCommandList()
    {
        return CommandList.Select(x => x.Name).ToList().Except(GetRequiredCommandList(), StringComparer.OrdinalIgnoreCase).ToList();
    }

    public virtual Dictionary<string,string> GetModuleCommandPairs()
    {
        return new Dictionary<string, string>() //keys are the command name, values are the corresponding modules
        {
            {"datachunking", "DataChunk" },
            {"execute_bof", "BofExecution" }, 
        };
    }

    public virtual List<string> GetOptionalModules()
    {
       return new List<string>() { "SleepEncrypt", "BofExecution", "DataChunk" };
    }

    public virtual List<string> GetPostExCommands()
    {
        return new List<string>() { "jump", "spawn", "inject" };
    }

    public virtual List<CommandItem> DisplayHelp(Dictionary<string, string> input)
    {
        if (!input.TryGetValue("/command", out var command))
        {
            return CommandList;
        }
        else
        {
            List<CommandItem> output = new List<CommandItem>();
            foreach (CommandItem item in CommandList)
            {
                if (item.Name.ToLower() == command.ToLower())
                {
                    output.Add(item);
                }
            }
            return output;
        }
    }

    public virtual bool ValidateCommand(string input, out Dictionary<string,string> args , out string error)
    {
        args = new Dictionary<string, string>();
        string command = input.Split(' ')[0];
        List<string> argsToParse = input.Split(' ').Skip(1).ToList();

        //if argsToParse contains /SkipCheck then the command and key validations are skipped 
        if (argsToParse.Contains("/SkipCheck"))
        {
            args = argsToParse.ToDictionary(x => x, x => "");
            error = null;
            return true;
        }


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
        if (commandItem.Keys == null || commandItem.Keys.Count == 0)
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

    public virtual List<string> GetContextChangingCommands()
    {
        return new List<string>() { "getsystem", "make_token", "steal_token", "rev2self" };
    }

    public virtual List<CommandItem> CommandList { get; } = new List<CommandItem>()
    {
        // new CommandItem()
        // {
        //      Name = "",
        //      Description = "",
        //      Usage = "commandName /key value",
        //      NeedsAdmin = false,
        //      Opsec = OpsecStatus.NotSet,
        //      MitreTechnique = "",        
        //      Keys = {{"/key1",false},{"/key2",false},}
        // },
        new CommandItem()
        {
            Name = "Addcommand",
            Description = "takes a .cs file from the engineer/Commands , compiles it, and then sends it to the engineer to be loaded as new commmand.",
            Usage = "addcommand /command value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey() {Name = "/command",Description = "name of the c# file to compile and send to the engineer" , Required = true, inputType = CommandKey.InputType.Text, PreDefinedValues = null, NeedsValues = true},
                    }
        },
        new CommandItem()
        {
            Name = "Addmodule",
            Description = "takes a .cs file from the engineer where the class has the Module Attribute , compiles it, and then sends it to the engineer to be loaded as new module.",
            Usage = "addmodule /module value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey() {Name = "/module",Description = "", Required = true, inputType = CommandKey.InputType.Text, PreDefinedValues = null, NeedsValues = true},
                    }
        },
        new CommandItem()
        {
            Name = "Add-MachineAccount",
            Description = "adds a machine account to the domain, can provide optional username and password to authenticate to the domain / other domains",
            Usage = "Add-MachineAccount /name value /machinePassword value /domain value /username value /password value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1136",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    { 
                        new CommandKey() {Name = "/username",Description = "", Required = false, inputType = CommandKey.InputType.Text, PreDefinedValues = null, NeedsValues = true},
                        new CommandKey("/password", "", false,CommandKey.InputType.Text, null, true),
                        new CommandKey("/domain","", false,CommandKey.InputType.Text, null, true),
                        new CommandKey("/name","", true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/machinePassword","",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "arp",
            Description = "executes the built in arp tool to return arp table",
            Usage = "arp",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1049",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null //no keys
        },
        new CommandItem()
        {
            Name = "cat",
            Description = "reads the target file ",
            Usage = "cat /file value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1083",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file","",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "cd",
            Description = "changes the current working directory",
            Usage = "cd /path value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1083",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/path","",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "copy",
            Description = "copy a file from one location to another",
            Usage = "copy /file value /dest value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1105",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/dest","",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "connect",
            Description = "starts a tcp server on the current Engineer, or connects into a existing TCP Engineer",
            Usage = "connect /ip value /port value /localhost value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1095",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/ip","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/port","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/localhost","",false,CommandKey.InputType.Text, null,  true)
                    }
        },
        //new CommandItem()
        //{
        //    Name = "cleanupinteractiveprofile",
        //    Keys = new List<CommandKey>()
        //    {
        //                new CommandKey("/sid",true,InputType.Text, null,  true)
        //    }
        //},
        //new CommandItem()
        //{
        //    Name = "createprocess_stolentoken",
        //    Keys = new List<CommandKey>()
        //    {
        //        new CommandKey("/program",true,InputType.Text, null,  true),
        //        new CommandKey("/args",false,InputType.Text, null,  true),
        //        new CommandKey("/index",true,InputType.Text, null,  true)
        //    }
        //},
         new CommandItem()
        {
            Name = "datachunking",
            Description = "Controls the data chunking settings, and retrieves the current settings and module status",
            Usage = "DataChunking /enable /disable /size value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/enable","",false,CommandKey.InputType.Text, null,  false),
                        new CommandKey("/disable","",false,CommandKey.InputType.Text, null,  false),
                        new CommandKey("/size","",false,CommandKey.InputType.Text, null,  true),
                    }
        },
        new CommandItem()
        {
            Name = "delete",
            Description = "removes a file",
            Usage = "delete /file value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1070",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file","",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "download",
            Description = "downloads the target file to the teamserver",
            Usage = "download /file value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1105",
            RequiresPreProc = false,
            RequiresPostProc = true,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file","",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "editfile",
            Description = "downloads the file and opens it in an editor to allow viewing or editing of the file",
            Usage = "editfile /file value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1105",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
            {
                        new CommandKey("/file","the file on the target to edit, will open file in a read only state if the user does not permission to edit the file",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "execute",
            Description = "spawns a target process with arguments but no output is returned",
            Usage = "execute /command value /args value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1059",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/command","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/args","",false,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "executeAssembly",
            Description = "executes the provided assembly in memory of the spawn to process, it is injected as shellcode and then executed",
            Usage = "executeAssembly /file value /args value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1055",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/args","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/local","",false,CommandKey.InputType.File, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "execute_bof",
            Description = "executes the provided COFF/BOF file inline on the engineer",
            Usage = "execute_bof /file value /function value /argtypes value /args value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1055",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/function","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/argtypes","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/args","",false,CommandKey.InputType.Text, null,  true),
                       // new CommandKey("/local",false,InputType.File, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "execute_pe",
            Description = "executes the provided PE file inline on the engineer",
            Usage = "execute_pe /file value /args value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1055",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/args","",false,CommandKey.InputType.Text, null,  true),
                       // new CommandKey("/local",false,InputType.File, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "exit",
            Description = "Exits the program",
            Usage = "exit",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.High,
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null
        },
        new CommandItem()
        {
            Name = "GetCommands",
            Description = "returns the current engineers loaded commands",
            Usage = "GetCommands",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null
        },
        new CommandItem()
        {
            Name = "get_luid",
            Description = "returns the current luid for the user",
            Usage = "get_luid",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null
        },
        new CommandItem()
        {
            Name = "getprivs",
            Description = "returns the current token privileges",
            Usage = "getprivs",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null
        },
        new CommandItem()
        {
            Name = "GetMachineAccountQuota",
            Description = "gets the machine account quota for the domain / other domains",
            Usage = "GetMachineAccountQuota /domain value /username value /password value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/domain","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/username","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/password","",false,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "GetModules",
            Description = "returns the current engineers loaded Modules",
            Usage = "GetModules",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null
        },
        new CommandItem()
        {
            Name = "getsystem",
            Description = "Elevates current process to SYSTEM or executes command as SYSTEM",
            Usage = "getSystem /elevate /command value",
            NeedsAdmin = true,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1134",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
            {
                        new CommandKey("/elevate","",false,CommandKey.InputType.Text, null,  false),
                        new CommandKey("/command","",false,CommandKey.InputType.Text, null,  false),
                    }
        },
        new CommandItem()
        {
            Name = "help",
            Description = "Displays this help menu",
            Usage = "help /command value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/command","",false,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "inject",
            Description = "injects shellcode of a engineer that matches the selected manager into the target pid",
            Usage = "inject /manager value /pid value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1055",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/manager","",true,CommandKey.InputType.Manager, IImplantCommandValidation.ManagerNames,  true),
                        new CommandKey("/pid","",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "inlineAssembly",
            Description = "runs the target assembly in memory with the supplied arguments",
            Usage = "inlineAssembly /file value /args value /execmethod OptionalValue /appdomain OptionalValue",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/args","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/execmethod","",false,CommandKey.InputType.DropDown, new List<string>(){"UnloadDomain","Classic"},  true),
                        new CommandKey("/appdomain","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/Patch_A","",false,CommandKey.InputType.CheckBox,null,false),
                        new CommandKey("/local","",false,CommandKey.InputType.File, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "inlineDll",
            Description = "runs the target dll in memory with the supplied arguments",
            Usage = "inlineDll /dll value /function value /args value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1055",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/dll","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/function","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/args","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/local","",false,CommandKey.InputType.File, null,  true)
                    }
        },
         new CommandItem()
        {
            Name = "InlineShellcode",
            Description = "injects shellcode into the current engineers process and runs it",
            Usage = "InlineShellcode /program value /args value /local value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1055",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/program","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/args","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/local","",false,CommandKey.InputType.File, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "ipconfig",
            Description = "gets a list of all ip addresses & masks on the target machine",
            Usage = "ipconfig",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null
        },
        new CommandItem()
        {
            Name = "jump",
            Description = "lateral movement onto the target machine using a few various techniques",
            Usage = "jump /method value /target value /manager value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1021",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/method","",true,CommandKey.InputType.DropDown, new List<string>(){"psexec","wmi","wmi-ps","winrm","dcom"},  true),
                        new CommandKey("/target","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/manager","",true,CommandKey.InputType.Manager, IImplantCommandValidation.ManagerNames,  true)
                    }
        },
        new CommandItem()
        {
            Name = "ldapSearch",
            Description = "performs an ldap search on the current domain or provided domain",
            Usage = "ldapSearch /search value /domain value /username value /password value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1087",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/search","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/domain","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/username","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/password","",false,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "ldapwhoami",
            Description = "performs an ldap whoami on the current domain or provided domain",
            Usage = "ldapwhoami /domain value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1087",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/domain","",false,CommandKey.InputType.Text, null,  true),
                    }
        },
        new CommandItem()
        {
            Name = "link",
            Description = "Used to connect to SMB engineers either in reverse or bind connections",
            Usage = "link /pipe value /ip optionalValue",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1570,T1572",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/pipe","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/ip","",false,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "loadAssembly",
            Description = "gets the provided assembly from the teamserver and loads it into the current process, uses D/Invoke to map to memory, and then invoke the main function EXPERIMENTAL",
            Usage = "loadAssembly /file value /args value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1055",
            RequiresPreProc = true,
            RequiresPostProc = false,
              Keys = new List<CommandKey>()
                      {
                            new CommandKey("/file","",true,CommandKey.InputType.Text, null,  true),
                            new CommandKey("/args","",false,CommandKey.InputType.Text, null,  true),
                            new CommandKey("/local","",false,CommandKey.InputType.File, null,  true)
                      }
        },
        new CommandItem()
        {
            Name = "ls",
            Description = "lists the contents of a directory",
            Usage = "ls /path optionalValue /getcount optionalValue /getacls optionalValue ",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1083",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/path","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/getcount","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/getacls","",false,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "make_token",
            Description = "creates a new token with the provided creds good for remote access",
            Usage = "make_token /username value /password value /domain value", ///localauth
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1134",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/username","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/password","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/domain","",true,CommandKey.InputType.Text, null,  true),
                        //new CommandKey("/localauth",false,InputType.Text, null,  false)
                    }
        },
        new CommandItem()
        {
            Name = "mimikatz",
            Description = "injects mimikatz dll into the local process using d/invoke which will perform module overload to try and hide it.",
            Usage = "mimikatz /args Value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/args","",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "mkdir",
            Description = "creates a new directory",
            Usage = "mkdir /path value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1083",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/path","",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "move",
            Description = "moves the source file to the destination",
            Usage = "move /file value /dest value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1083",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/file","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/dest","",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "net-localgroup",
            Description = "returns a list of all local groups on the current machine",
            Usage = "net-localgroup",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1087",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null
        },
        new CommandItem()
        {
            Name = "net-localgroup-members",
            Description = "returns a list of all members of the specified local group on the current machine, if no group is specified it will return all members of all local groups",
            Usage = "net-localgroup-members /group value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1087",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/group","",false,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "net-Dclist",
            Description = "returns a list of all domain controllers on the current domain, or the domain specified",
            Usage = "net-Dclist /domain value /username value /password value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1087",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/domain","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/username","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/password","",false,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "patch_amsi",
            Description = "patches amsi in the current process using d/invoke",
            Usage = "patch_amsi",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1562",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null
        },
        new CommandItem()
        {
            Name = "patch_etw",
            Description = "patches etw in the current process using d/invoke",
            Usage = "patch_etw",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1562",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null
        },
        new CommandItem()
        {
            Name = "powershell_import",
            Description = "imports a powershell script into the engineer to run with powershell, unmanaged powershell",
            Usage = "powershell_import /import value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1059",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/import","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/remove","",false,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/local","",false,CommandKey.InputType.File, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "powerlist",
            Description = "lists the currently imported powershell scripts",
            Usage = "powerlist",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1059",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null
        },
        new CommandItem()
        {
            Name = "ps",
            Description = "lists all running processes, the arch, session and owner if possible",
            Usage = "ps",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1057",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null
        },
        new CommandItem()
        {
            Name = "print-env",
            Description = "lists all the environment variables",
            Usage = "print-env",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1082",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null
        },
        new CommandItem()
        {
            Name = "pwd",
            Description = "prints the current working directory",
            Usage = "pwd",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1083",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null
        },
        new CommandItem()
        {
            Name = "rev2self",
            Description = "reverts the current token back to the orginal",
            Usage = "rev2self",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1134",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = null
        },
        new CommandItem()
        {
            Name = "rportforward",
            Description = "sets a reverse port forward from the implant to the teamserver, which then sends the data to the specified host and port",
            Usage = "rportForward /fwdhost value /fwdport value /bindport value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1571",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/fwdport","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/fwdhost","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/bindport","",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "run",
            Description = "executes the target program with the supplied arguments and returns the output to the C2",
            Usage = "run /command value /args value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1059",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/command","",true,CommandKey.InputType.Text, null,  true),
                        new CommandKey("/args","",false,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "runas",
            Description = "executes the target program with the supplied arguments and returns the output to the C2, using the supplied credentials",
            Usage = "runas /program value /args value /username value /password value /domain value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1059",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
            {
                new CommandKey("/program","",true,CommandKey.InputType.Text, null,  true),
                new CommandKey("/args","",false,CommandKey.InputType.Text, null,  true),
                new CommandKey("/username","",true,CommandKey.InputType.Text, null,  true),
                new CommandKey("/password","",true,CommandKey.InputType.Text, null,  true),
                new CommandKey("/domain","",true,CommandKey.InputType.Text, null,  true),
            }
        },
        new CommandItem()
        {
            Name = "shell",
            Description = "uses cmd.exe /c to execute the supplied command",
            Usage = "shell /command value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1059",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/command","",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "sleep",
            Description = "sets the sleep time for the engineer",
            Usage = "sleep /time value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1029",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/time","",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "socks",
            Description = "starts a socks server on the teamserver at the specified port",
            Usage = "socks /port value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1571",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
                    {
                        new CommandKey("/port","",true,CommandKey.InputType.Text, null,  true)
                    }
        },
        new CommandItem()
        {
            Name = "spawn",
            Description = "starts the spawnto program and injects shellcode into it from an enginner that matches the selected manager",
            Usage = "spawn /manager value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1055",
            RequiresPreProc = true,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
            {
                new CommandKey("/manager","",true,CommandKey.InputType.Manager, IImplantCommandValidation.ManagerNames,  true)
            }
        },
        new CommandItem()
        {
            Name = "spawnto",
            Description = "the target program for spawn",
            Usage = "spawnto /path value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
            {
                new CommandKey("/path","",true,CommandKey.InputType.Text, null,  true)
            }
        },
        new CommandItem()
        {
            Name = "steal_token",
            Description = "steal a token for a user on the system",
            Usage = "steal_token /pid value",
            NeedsAdmin = true,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1134",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
            {
                new CommandKey("/pid","",true,CommandKey.InputType.Text, null,  true)
            }
        },
        new CommandItem()
        {
            Name = "token_store",
            Description = "stores a token for later use",
            Usage = "token_store /view /use value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1134",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
            {
                new CommandKey("/remove","",false,CommandKey.InputType.Text, null,  true),
                new CommandKey("/view","",false,CommandKey.InputType.Text, null,  false),
                new CommandKey("/use","",false,CommandKey.InputType.Text, null,  true),
            }
        },
        new CommandItem()
        {
            Name = "unmanagedPowershell",
            Description = "uses a powershell runner to execute the supplied command",
            Usage = "unmanagedpowershell /command value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1059",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
            {
                new CommandKey("/command","",true,CommandKey.InputType.Text, null,  true)
            }
        },
        new CommandItem()
        {
            Name = "upload",
            Description = "uploads a file to the target machine",
            Usage = "upload /file value /dest value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1105",
            RequiresPreProc = true,
            RequiresPostProc = true,
            Keys = new List<CommandKey>()
            {
                new CommandKey("/file","",true,CommandKey.InputType.Text, null,  true),
                new CommandKey("/dest","",true,CommandKey.InputType.Text, null,  true),
                new CommandKey("/local","",false,CommandKey.InputType.File, null,  true)
            }
        },
        new CommandItem()
        {
            Name = "viewAssembly",
            Description = "downloads and decompiles the target file into source code",
            Usage = "viewAssembly /file value",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1105",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
            {
                new CommandKey("/file","the target file to try and decompile, if it is successful it opens in a edit file window with a file browser to navigate the decompiled classes",true,CommandKey.InputType.Text, null,  true)
            }
        },
        //new CommandItem()
        //{
        //    Name = "vnc",
        //    Description = "gets data of the screen and renders it as an image back on the client",
        //    Usage = "vnc /operation start",
        //    NeedsAdmin = false,
        //    Opsec = OpsecStatus.NotSet,
        //    MitreTechnique = "",
        //    RequiresPreProc = true,
        //    RequiresPostProc = true,
        //    Keys = new List<CommandKey>()
        //    {
        //        new CommandKey("/operation","if the vnc session should be created or stopped",false,InputType.Text, new List<string>(){"start","stop"},  true)
        //    }
        //},
        new CommandItem()
        {
            Name = "whoami",
            Description = "gets the username of the current process, with /groups it will get the groups the user belongs too",
            Usage = "whoami /groups",
            NeedsAdmin = false,
            Opsec = CommandItem.OpsecStatus.NotSet,
            MitreTechnique = "T1033",
            RequiresPreProc = false,
            RequiresPostProc = false,
            Keys = new List<CommandKey>()
            {
                new CommandKey("/groups","",false,CommandKey.InputType.CheckBox, null,  false)
            }
        },
    };


}

public class ImplantCommandValidationBaseData : I_ImplantCommandValidationBaseData
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public interface IImplantCommandValidation : IClientPlugin
{
    public I_ImplantCommandValidationBaseData _metadata { get; set; }

    public static List<string> ManagerNames
    {
        get { return Managers.managersList.Select(manager => manager.Name).ToList(); }
    }

    public static Dictionary<string, List<string>> ImplantLoadedCommands = new Dictionary<string, List<string>>();

    List<CommandItem> CommandList { get; }
    bool ValidateCommand(string input, out Dictionary<string, string> args, out string error);
    List<string> GetPostExCommands();
    List<string> GetOptionalModules();
    Dictionary<string, string> GetModuleCommandPairs();
    List<CommandItem> DisplayHelp(Dictionary<string, string> input);
    List<string> GetOptionalCommandList();
    List<string> GetRequiredCommandList();
    List<string> GetContextChangingCommands();
}

public interface I_ImplantCommandValidationBaseData : IClientPluginData
{

}