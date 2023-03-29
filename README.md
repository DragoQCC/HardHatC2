# HardHat C2 
## A cross-platform, collaborative, Command & Control framework written in C#, designed for red teaming and ease of use.
![image](https://user-images.githubusercontent.com/15575425/228551034-e07df233-63f6-41a2-8b94-6eb840859e82.png)

HardHat is a multiplayer c# .NET-based command and control framework. Designed to aid in red team engagements and penetration testing. HardHat aims to improve the quality of life factors during engagements by providing an easy-to-use but still robust C2 framework.    
It contains three primary components, an ASP.NET teamserver, a blazor .NET client, and c# based implants.

# Relase Tracking 
Alpha Release - 3/29/23
 NOTE: HardHat is in Alpha release; it will have bugs, missing features, and unexpected things will happen. TThank you for trying it, and please report back any issues or missing features so they can be addressed.

# Features 
## Teamserver & Client 
- Per-operator accounts with account tiers to allow customized access control and features, including view-only guest modes, team-lead opsec approval(WIP), and admin accounts for general operation management. 
- Managers (Listeners) 
- Dynamic Payload Generation (Exe, Dll, shellcode, PowerShell command)
- Creation & editing of C2 profiles on the fly in the client
- Customization of payload generation 
  - sleep time/jitter 
  - kill date
  - working hours 
  - type (Exe, Dll, Shellcode, ps command)
  - Included commands(WIP)
  - option to run confuser
- File upload & Downloads 
- Graph View 
- File Browser GUI
- Event Log 
- JSON logging for events & tasks 
- Loot tracking (Creds, downloads)
- IOC tracing 
- Pivot proxies (SOCKS 4a, Port forwards)
- Cred store 
- Autocomplete command history 
- Detailed help command 
- Interactive bash terminal command if the client is on linux or powershell on windows, this allows automatic parsing and logging of terminal commands like proxychains
- Persistent database storage of teamserver items (User accounts, Managers, Engineers, Events, tasks, creds, downloads, uploads, etc. )
- Recon Entity Tracking (track info about users/devices, random metadata as needed)
- Shared files for some commands (see teamserver page for details)
- tab-based interact window for command issuing 
- table-based output option for some commands like ls, ps, etc. 
- Auto parsing of output from seatbelt to create "recon entities" and fill entries to reference back to later easily 
- Dark and Light 🤮 theme 

 ![image](https://user-images.githubusercontent.com/15575425/228551170-cd455c24-3541-47ec-ad85-dcb84ce64075.png)
![image](https://user-images.githubusercontent.com/15575425/228551467-750a5a3a-dcff-4290-968e-7b18598e74b6.png)

 
## Engineers
- c# .net framework implant for windows devices, currently only CLR/.NET 4 support
- atm only one implant, but looking to add others 
- It can be generated as EXE, DLL, shellcode, or PowerShell stager
- Rc4 encryption of payload memory & heap when sleeping (Exe / DLL only)
- AES encryption of all network communication 
- ConfuserEx integration for obfuscation
- HTTP, HTTPS, TCP, SMB communication
  - TCP & SMB can work P2P in a bind or reverse setups
- Unique per implant key generated at compile time 
- multiple callback URI's depending on the C2 profile 
- P/Invoke & D/Invoke integration for windows API calls 
- SOCKS 4a support 
- Reverse Port Forward & Port Forwards 
- All commands run as async cancellable jobs 
  - Option to run commands sync if desired
- Inline assembly execution & inline shellcode execution
- DLL Injection 
- Execute assembly & Mimikatz integration
- Mimikatz is not built into the implant but is pushed when specific commands are issued
- Various localhost & network enumeration tools 
- Token manipulation commands 
  - Steal Token Mask(WIP) 
- Lateral Movement Commands 
- Jump (psexec, wmi, wmi-ps, winrm, dcom)
- Remote Execution (WIP)
- AMSI & ETW Patching 
- Unmanaged Powershell 
- Script Store (can load multiple scripts at once if needed)
- Spawn & Inject 
  - Spawn-to is configurable 
- run, shell & execute
![image](https://user-images.githubusercontent.com/15575425/228551103-0f1fe1f5-9b2d-42f9-a22d-f929f17b3b93.png)

# Documentation
documentation can be found at [docs](https://docs.hardhat-c2.net/)

# Getting Started 
## Prerequisites
- Installation of the [.net 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) from Microsoft 
- Once installed, the teamserver and client are started with dotnet run
### Teamserver
To configure the team server's starting address (where clients will connect), edit the HardHatC2\TeamServer\Properties\LaunchSettings.json changing the "applicationUrl": "https://127.0.0.1:5000" to the desired location and port. 
start the teamserver with dotnet run from its top-level folder ../HrdHatC2/Teamserver/

### HardHat Client 
1. When starting the client to set the target teamserver location, include it in the command line dotnet run https:\\127.0.0.1:5000 for example 
2. open a web browser and navigate to https://localhost:7096/ if this works, you should see the login page 
3. Log in with the HardHat_Admin user (Password is printed on first TeamServer startup)
4. Navigate to the settings page & create a new user if successful, a  message should appear, then you may log in with that account to access the full client


# Community 
[Discord](https://discord.gg/npW2yy7JFK) 
Join the community to talk about HardHat C2, Programming, Red teaming and general cyber security things 
The discord community is also a great way to request help, submit new features, stay up to date on the latest additions, and submit bugs. 

# Controbutions & Bug Reports 
 Code contributions are welcome feel free to submit feature request, pull requests or send me your ideas on discord. 
