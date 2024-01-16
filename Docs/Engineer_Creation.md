Engineers 
An engineer is an implant that is meant to be deployed on a target system for post-compromise exploitation, enumeration, and lateral movement. 
All engineer payloads are stageless atm the PowerShell command downloads the whole hosted implant and executes it.
Engineers can be tracked and created via the Engineers page in the client and interacted with via the interact page. This allows for a larger dedicated space to interact with the engineers. 
At current, Engineers only target .net 4 x64 , .net CORE and some other implant types are in the works

Engineers Page 
Engineers Table 

The engineers' table is where operators can see all the current and old engineers that have called back. The table displays some common metadata for the implants. 
Integrity Level Icon
The far left end of the table is an icon of a computer, color-coded depending on the int level. Low = Green, Medium = Blue, high = Orange, Red = System. 
External Address
This is the address that the network traffic came into the teamserver from, so if you are using a redirector, this should match the redirector address. 
Manager 
The manager this engineer is set to use configures the callback address and C2 Profile. 
Connection Type 
The type of engineer (HTTP, HTTPS, TCP, SMB)
Address 
the internal address of the implant
Hostname 
The name of the computer 
Username 
The full username of the user, including the domain or hostname, depending on it is a local or domain account 
process 
the name of the process the engineer is running in 
PID
the process id of the current running process 
Integrity 
the current integrity level of the implant (SYSTEM, HIGH, MEDIUM, LOW)
Arch 
the architecture of the process 
Sleep Time 
The current sleep time setting 
Last seen 
how long has it been since the implant last checked in
Options Menu 
Allows for Interaction, Deletion, adding a note, or highlighting the row with a specific color 
Engineer Creation Form 

Manager Name 
A dropdown list to select the manager you want to base the engineer on 
Connection attempts 
the number of connections that the engineer should attempt if it can't reach the teamserver before exiting, for example, a sleep time of 5 seconds, and 1000 attempts will take 5000 seconds before it stops trying to connect back.  
Sleep Time 
the amount in seconds the implant should sleep for. This can be changed with the sleep command later if needed. 
Working Hours - Optional 
Sets the hours an implant should perform routine check-ins in a UTC timestamp, for example 13:00:00-16:00:00 UTC means the implant will only check in during those hours, and once it hits, 16:00:01 it will sleep until 13:00:00 the next day.   Take care not to set this to a Local time zone like EST because it is calculated on UTC, which will cause the implant to stop at a different time than you intended. 
Compile Type 
This is the output format for the engineer (EXE, Service EXE, DLL, SHELLCODE, PS Command) 
the ps command compile option will fill the powershell commnad box at the bottom of the form. It also uploads a ps1 file containing the command to the teamserver web server root folder. See the uploaded files section for info on accessing it & a copy to the main HardHatC2 folder on the teamserver.  This contains a command to run that will download the hosted ps file from the teamserver and run it. This is not a stager. The whole engineer byte array is downloaded and executed. 

Engineer Compilation 
Engineers are dynamically compiled on the teamserver taking in the info from the creation form, and built with Roslyn .NET compiler. Then any required DLLs are merged into the assembly by default (FastJSON) using ilrepacking to create the final implant file. 
Once compiled, the engineer is saved in the main HardHatC2 folder on the teamserver for quick access by any team member. By default, the name is Engineer_Managername so an engineer EXE with the manager named demo would have the filename Engineer_Demo.exe . 
The shellcode option uses Donut to turn the EXE to shellcode. The shellcode is also then passed to sgn for an encoded version as well.    Note: Currently, HardHat uses a stock version of Donut 1.0 which will carry some indicators in the final shellcode. 