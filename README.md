![Discord](https://img.shields.io/discord/882642569207644200?logo=discord&label=Discord)
![GitHub issues](https://img.shields.io/github/issues/DragoQCC/HardHatC2)
![GitHub Repo stars](https://img.shields.io/github/stars/DragoQcc/HardHatC2?style=social)
![GitHub forks](https://img.shields.io/github/forks/Dragoqcc/HardHatC2?style=social)
![GitHub tag (latest by date)](https://img.shields.io/github/v/tag/DragoQcc/HardHatc2)
![GitHub last commit](https://img.shields.io/github/last-commit/dragoqcc/hardhatc2)
![Twitter Follow](https://img.shields.io/twitter/follow/dragoqcc)
<a href="https://bloodhoundgang.herokuapp.com/">
  <img src="https://img.shields.io/badge/BloodHound Slack-4A154B?logo=slack&logoColor=white" alt="chat on Bloodhound Slack" />
</a>
<a href="https://github.com/specterops#hardhatc2">
  <img src="https://img.shields.io/endpoint?url=https%3A%2F%2Fraw.githubusercontent.com%2Fspecterops%2F.github%2Fmain%2Fconfig%2Fshield.json" alt="Sponsored by SpecterOps"/>
</a>

# HardHat C2

## A cross-platform, collaborative, Command & Control framework written in C#, designed for red teaming and ease of use

![image](https://user-images.githubusercontent.com/15575425/228551034-e07df233-63f6-41a2-8b94-6eb840859e82.png)

HardHat is a multi-user C# .NET-based command and control (C2) framework designed to aid in red team engagements and penetration testing. It aims to improve quality-of-life during engagements by providing a robust, easy-to-use C2 framework.

HardHat has three main components:

1. An ASP.NET teamserver
2. A Blazor .NET client
3. Built-in C# based implants
   - Support for 3rd party implants in other languages   

Full documentation is available at [https://docs.hardhat-c2.net/](https://docs.hardhat-c2.net/).

**NOTE**: HardHat is in an Alpha release; it will have bugs, missing features, and unexpected things will happen. Thank you for trying it, and please report back any issues or missing features so they can be addressed.

## Community

Join our [Discord][Discord] community to talk about HardHat C2, programming, red teaming and general cyber security topics. It's also a great place to ask for help, submit bugs or new features, and stay up-to-date on the latest additions.

Code contributions are welcome! Feel free to submit feature requests, pull requests, or send me your ideas on [Discord][Discord].

## Features
### Custom Asset Support 
- Assets are the Implants and associated plugins for the team server and client. 
To see the available ones and learn how to create more, check out the [HardHat Toolbox](https://github.com/HardHatToolbox)

### Teamserver & Client

- Individual operator accounts with role-based access control (RBAC)
  - Allows account personalization
  - Allows restricted access to specific features (e.g., view-only guest role, team-lead opsec approval (WIP))
- Managers (Listeners)
- Dynamic Payload Generation (EXE, DLL, shellcode, PowerShell command)
- Creation & editing of C2 profiles on the fly in the client
- Customization of payload generation
  - Sleep time/jitter
  - Kill date
  - Working hours
  - Type (EXE, DLL, shellcode, PowerShell command)
  - Included commands (WIP)
  - Option to run [ConfuserEx][ConfuserEx]
- File upload & Downloads
- Graph View
- File Browser GUI
- Event Log
- JSON logging for events & tasks
- Loot tracking
  - Credentials
  - Downloads
- Indicator of Compromise (IOC) tracking
- Pivot proxies (SOCKS 4a, Port forwards)
- Credential store
- Autocomplete command history
- Detailed help command
- Interactive bash terminal command if the client is on Linux or PowerShell on Windows
  - Allows automatic parsing and logging of terminal commands like proxychains
- Persistent database storage of teamserver items (User accounts, Managers, Engineers, Events, tasks, creds, downloads, uploads, etc. )
- Recon Entity Tracking (track info about users/devices, random metadata as needed)
- Shared files for some commands (see teamserver page for details)
- tab-based interact window for issuing commands
- Table-based output option for some commands (e.g., `ls`, `ps`, etc.)
- Automatic parsing of [Seatbelt](https://github.com/GhostPack/Seatbelt) output to create "recon entities" for convenient reference
- Dark and Light 🤮 theme

![image](https://user-images.githubusercontent.com/15575425/228551170-cd455c24-3541-47ec-ad85-dcb84ce64075.png)
![image](https://user-images.githubusercontent.com/15575425/228551467-750a5a3a-dcff-4290-968e-7b18598e74b6.png)

### Engineers

- C# .NET framework implant for Windows devices (currently only CLR/.NET 4 support)
  - Only one implant at the moment, but looking to add others
- Can be generated as EXE, DLL, shellcode, or PowerShell stager
- RC4 encryption of payload memory & heap when sleeping (EXE / DLL only)
- AES encryption of all network communication
- [ConfuserEx][ConfuserEx] integration for obfuscation
- HTTP, HTTPS, TCP, SMB communication
  - TCP & SMB can work peer-to-peer (P2P) in bind or reverse configurations
- Unique per implant key generated at compile time
- Multiple callback URI's depending on the C2 profile
- P/Invoke & D/Invoke integration for windows API calls
- SOCKS 4a support
- Reverse Port Forward & Port Forwards
- All commands run as asynchronous, cancellable jobs
  - Option to run commands synchronously, if desired
- Inline assembly execution & inline shellcode execution
- DLL Injection
- Execute assembly & Mimikatz integration
  - Mimikatz is not built into the implant but is pushed when specific commands are issued
- Various local and network enumeration tools
- Token manipulation commands
  - Steal Token Mask (WIP)
- Lateral Movement Commands
- Jump (psexec, wmi, wmi-ps, winrm, dcom)
- Remote Execution (WIP)
- Antimalware Scan Interface (AMSI) & Event Tracing for Windows (ETW) Patching
- Unmanaged Powershell
- Script Store allows multiple scripts to be loaded at once
- Spawn & Inject
  - Spawn-to is configurable
- Run, execute, and shell
![image](https://user-images.githubusercontent.com/15575425/228551103-0f1fe1f5-9b2d-42f9-a22d-f929f17b3b93.png)

## Getting Started

### Installation

#### Docker

1. Install Docker and Docker Compose
2. Run `docker compose up -d`
    - Optionally, provide `HARDHAT_ADMIN_USERNAME` and/or `HARDHAT_ADMIN_PASSWORD` as environment variables; if omitted, the default admin username and randomly generated password will be written to the teamserver logs on first run
3. Navigate to [https://localhost:7096/](https://localhost:7096/) in your browser

#### Manual

1. Install [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) from Microsoft
2. Run `dotnet run` from the `.\TeamServer` directory to build and start the teamserver
3. Run `dotnet run https://<TEAMSERVER_HOST>:<TEAMSERVER_PORT>` from the `HardHatC2Client` directory
    - For example, assuming your teamserver is running on the same host and default port: `dotnet run https://127.0.0.1:5000`
4. Navigate to [https://localhost:7096/](https://localhost:7096/) in your browser

To configure the teamserver's listening address (i.e., where clients will connect), edit `.\TeamServer\Properties\LaunchSettings.json` and change `"applicationUrl": "https://127.0.0.1:5000"` to the desired location and port.

### Setup

1. Login to the client web UI using the username and password set with environment variable or printed to STDOUT by the teamserver
2. Navigate to the Settings page and create a new user account
    - If successful, a message will appear; you may then login with that account to access the full client

## Release Tracking

- Alpha 0.2 Release - 7/6/23
  - Change log: <https://docs.hardhat-c2.net/changelog/alpha-0.2-update-july-6-2023>
- Alpha Release - 3/29/23

[ConfuserEx]: https://github.com/mkaring/ConfuserEx
[Discord]: https://discord.gg/npW2yy7JFK
