using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Engineer.Extra.h_reprobate.Win32;
using static Engineer.Extra.h_reprobate.Win32.WinNT;

namespace Engineer.Extra
{
    public class WinAPIs
    {
        public class Kernel32
        {
            [DllImport("Kernel32.dll")]
            public static extern bool CloseHandle(IntPtr hObject);

            [DllImport("kernel32.dll")]
            public static extern uint GetLastError();

            //dll import for readconsoleoutput
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool ReadConsoleOutput(IntPtr hConsoleOutput, [Out] CHAR_INFO[] lpBuffer, COORD dwBufferSize, COORD dwBufferCoord, ref SMALL_RECT lpReadRegion);

            //dll import for getstdhandle
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr GetStdHandle(int nStdHandle);

            //dll import for SetStdHandle
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);

            //dll import for OpenProcess
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

            //dll import for CreateRemoteThread 
            [DllImport("kernel32.dll")]
            public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

            //dll import for iswow64process
            [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

            [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
            public static extern bool FreeConsole();

            [DllImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
           public static extern bool AllocConsole();

            //dll import for CreatePipe
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

            //dll import for CreateNamedPipe
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr CreateNamedPipe(string lpName, PipeOpenModeFlags dwOpenMode, PipeModeFlags dwPipeMode, uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize,uint nDefaultTimeOut, ref SECURITY_ATTRIBUTES lpSecurityAttributes);

            [DllImport("kernel32.dll", EntryPoint = "PeekNamedPipe", SetLastError = true)]
            public static extern bool PeekNamedPipe(IntPtr handle,byte[] buffer, uint nBufferSize, ref uint bytesRead,ref uint bytesAvail, ref uint BytesLeftThisMessage);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr CreateFile([MarshalAs(UnmanagedType.LPTStr)] string filename,[MarshalAs(UnmanagedType.U4)] EFileAccess access,[MarshalAs(UnmanagedType.U4)] EFileShare share, IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            [MarshalAs(UnmanagedType.U4)] ECreationDisposition creationDisposition, [MarshalAs(UnmanagedType.U4)] EFileAttributes flagsAndAttributes,IntPtr templateFile);

            //dll import for read file
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool SetHandleInformation(IntPtr hObject, HANDLE_FLAGS dwMask,HANDLE_FLAGS dwFlags);

            //dll import for WaitNamedPipe
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool WaitNamedPipe(string name, uint timeout);
            
            //dll import for waitForSingleObject
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

            [Flags]
            public enum HANDLE_FLAGS : uint
            {
                None = 0,
                INHERIT = 1,
                PROTECT_FROM_CLOSE = 2
            }

            //set the consts for memory allocation and protection
            public const uint MEM_COMMIT = 0x1000;
            public const uint MEM_RESERVE = 0x2000;
            public const uint PAGE_EXECUTE_READWRITE = 0x40;
            public const uint PAGE_EXECUTE_READ = 0x20;
            public const uint PAGE_EXECUTE_WRITECOPY = 0x80;
            public const uint PAGE_EXECUTE = 0x10;
            public const uint PAGE_READONLY = 0x02;
            public const uint PAGE_READWRITE = 0x04;
            public const uint PAGE_WRITECOPY = 0x08;
            public const uint PAGE_NOACCESS = 0x01;
            public const uint PAGE_GUARD = 0x100;
            public const uint PAGE_NOCACHE = 0x200;
            public const uint PAGE_WRITECOMBINE = 0x400;


            //set the consts for the handle of stdin, stdout, stderr
            public const int STD_INPUT_HANDLE = -10;
            public const int STD_OUTPUT_HANDLE = -11;
            public const int STD_ERROR_HANDLE = -12;

            //set the consts for the handle of stdin, stdout, stderr
            public const int STD_INPUT_HANDLE_VALUE = -10;
            public const int STD_OUTPUT_HANDLE_VALUE = -11;
            public const int STD_ERROR_HANDLE_VALUE = -12;

            //set of consts for process flags 
            public const uint ProcessAllFlags = 0x001F0FFF;
            public const uint GenericAll = 0x10000000;
            public const uint PageReadWrite = 0x04;
            public const uint PageReadExecute = 0x20;
            public const uint PageReadWriteExecute = 0x40;
            public const uint SecCommit = 0x08000000;

            //load security_attributes struct
            [StructLayout(LayoutKind.Sequential)]
            public struct SECURITY_ATTRIBUTES
            {
                public int nLength;
                public IntPtr lpSecurityDescriptor;
                public bool bInheritHandle;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct STARTUPINFO
            {
                public int cb;
                public IntPtr lpReserved;
                public IntPtr lpDesktop;
                public IntPtr lpTitle;
                public int dwX;
                public int dwY;
                public int dwXSize;
                public int dwYSize;
                public int dwXCountChars;
                public int dwYCountChars;
                public int dwFillAttribute;
                public int dwFlags;
                public short wShowWindow;
                public short cbReserved2;
                public IntPtr lpReserved2;
                public IntPtr hStdInput;
                public IntPtr hStdOutput;
                public IntPtr hStdError;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct STARTUPINFOEX
            {
                public STARTUPINFO StartupInfo;
                public IntPtr lpAttributeList;
            }

            [Flags]
            public enum CreationFlags
            {
                CreateSuspended = 0x00000004,
                DetachedProcess = 0x00000008,
                CreateNoWindow = 0x08000000,
                ExtendedStartupInfoPresent = 0x00080000
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_INFORMATION
            {
                public IntPtr hProcess;
                public IntPtr hThread;
                public int dwProcessId;
                public int dwThreadId;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct COORD
            {
                public short X;
                public short Y;
            }

            //struct for char_info
            [StructLayout(LayoutKind.Explicit)]
            public struct CHAR_INFO
            {
                [FieldOffset(0)]
                public char UnicodeChar;
                [FieldOffset(0)]
                public char AsciiChar;
                [FieldOffset(2)]
                public short Attributes;
            }
            //struct for small_rect
            [StructLayout(LayoutKind.Sequential)]
            public struct SMALL_RECT
            {
                public short Left;
                public short Top;
                public short Right;
                public short Bottom;
            }

            [Flags]
            public enum PipeOpenModeFlags : uint
            {
                PIPE_ACCESS_DUPLEX = 0x00000003,
                PIPE_ACCESS_INBOUND = 0x00000001,
                PIPE_ACCESS_OUTBOUND = 0x00000002,
                FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000,
                FILE_FLAG_WRITE_THROUGH = 0x80000000,
                FILE_FLAG_OVERLAPPED = 0x40000000,
                WRITE_DAC = (uint)0x00040000L,
                WRITE_OWNER = (uint)0x00080000L,
                ACCESS_SYSTEM_SECURITY = (uint)0x01000000L
            }
            
            [Flags]
            public enum PipeModeFlags : uint
            {
                //One of the following type modes can be specified. The same type mode must be specified for each instance of the pipe.
                PIPE_TYPE_BYTE = 0x00000000,
                PIPE_TYPE_MESSAGE = 0x00000004,
                //One of the following read modes can be specified. Different instances of the same pipe can specify different read modes
                PIPE_READMODE_BYTE = 0x00000000,
                PIPE_READMODE_MESSAGE = 0x00000002,
                //One of the following wait modes can be specified. Different instances of the same pipe can specify different wait modes.
                PIPE_WAIT = 0x00000000,
                PIPE_NOWAIT = 0x00000001,
                //One of the following remote-client modes can be specified. Different instances of the same pipe can specify different remote-client modes.
                PIPE_ACCEPT_REMOTE_CLIENTS = 0x00000000,
                PIPE_REJECT_REMOTE_CLIENTS = 0x00000008
            }
            [Flags]
            public enum EFileAccess : uint
            {
                //
                // Standart Section
                //
                AccessSystemSecurity = 0x1000000,   // AccessSystemAcl access type
                MaximumAllowed = 0x2000000,     // MaximumAllowed access type
                Delete = 0x10000,
                ReadControl = 0x20000,
                WriteDAC = 0x40000,
                WriteOwner = 0x80000,
                Synchronize = 0x100000,

                StandardRightsRequired = 0xF0000,
                StandardRightsRead = ReadControl,
                StandardRightsWrite = ReadControl,
                StandardRightsExecute = ReadControl,
                StandardRightsAll = 0x1F0000,
                SpecificRightsAll = 0xFFFF,

                FILE_READ_DATA = 0x0001,        // file & pipe
                FILE_LIST_DIRECTORY = 0x0001,       // directory
                FILE_WRITE_DATA = 0x0002,       // file & pipe
                FILE_ADD_FILE = 0x0002,         // directory
                FILE_APPEND_DATA = 0x0004,      // file
                FILE_ADD_SUBDIRECTORY = 0x0004,     // directory
                FILE_CREATE_PIPE_INSTANCE = 0x0004, // named pipe
                FILE_READ_EA = 0x0008,          // file & directory
                FILE_WRITE_EA = 0x0010,         // file & directory
                FILE_EXECUTE = 0x0020,          // file
                FILE_TRAVERSE = 0x0020,         // directory
                FILE_DELETE_CHILD = 0x0040,     // directory
                FILE_READ_ATTRIBUTES = 0x0080,      // all
                FILE_WRITE_ATTRIBUTES = 0x0100,     // all

                //
                // Generic Section
                //

                GenericRead = 0x80000000,
                GenericWrite = 0x40000000,
                GenericExecute = 0x20000000,
                GenericAll = 0x10000000,

                SPECIFIC_RIGHTS_ALL = 0x00FFFF,
                FILE_ALL_ACCESS =
                StandardRightsRequired |
                Synchronize |
                0x1FF,

                FILE_GENERIC_READ =
                StandardRightsRead |
                FILE_READ_DATA |
                FILE_READ_ATTRIBUTES |
                FILE_READ_EA |
                Synchronize,

                FILE_GENERIC_WRITE =
                StandardRightsWrite |
                FILE_WRITE_DATA |
                FILE_WRITE_ATTRIBUTES |
                FILE_WRITE_EA |
                FILE_APPEND_DATA |
                Synchronize,

                FILE_GENERIC_EXECUTE =
                StandardRightsExecute |
                  FILE_READ_ATTRIBUTES |
                  FILE_EXECUTE |
                  Synchronize
            }

            [Flags]
            public enum EFileShare : uint
            {
                /// <summary>
                ///
                /// </summary>
                None = 0x00000000,
                /// <summary>
                /// Enables subsequent open operations on an object to request read access.
                /// Otherwise, other processes cannot open the object if they request read access.
                /// If this flag is not specified, but the object has been opened for read access, the function fails.
                /// </summary>
                Read = 0x00000001,
                /// <summary>
                /// Enables subsequent open operations on an object to request write access.
                /// Otherwise, other processes cannot open the object if they request write access.
                /// If this flag is not specified, but the object has been opened for write access, the function fails.
                /// </summary>
                Write = 0x00000002,
                /// <summary>
                /// Enables subsequent open operations on an object to request delete access.
                /// Otherwise, other processes cannot open the object if they request delete access.
                /// If this flag is not specified, but the object has been opened for delete access, the function fails.
                /// </summary>
                Delete = 0x00000004
            }

            public enum ECreationDisposition : uint
            {
                /// <summary>
                /// Creates a new file. The function fails if a specified file exists.
                /// </summary>
                New = 1,
                /// <summary>
                /// Creates a new file, always.
                /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes,
                /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
                /// </summary>
                CreateAlways = 2,
                /// <summary>
                /// Opens a file. The function fails if the file does not exist.
                /// </summary>
                OpenExisting = 3,
                /// <summary>
                /// Opens a file, always.
                /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
                /// </summary>
                OpenAlways = 4,
                /// <summary>
                /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
                /// The calling process must open the file with the GENERIC_WRITE access right.
                /// </summary>
                TruncateExisting = 5
            }

            [Flags]
            public enum EFileAttributes : uint
            {
                Readonly = 0x00000001,
                Hidden = 0x00000002,
                System = 0x00000004,
                Directory = 0x00000010,
                Archive = 0x00000020,
                Device = 0x00000040,
                Normal = 0x00000080,
                Temporary = 0x00000100,
                SparseFile = 0x00000200,
                ReparsePoint = 0x00000400,
                Compressed = 0x00000800,
                Offline = 0x00001000,
                NotContentIndexed = 0x00002000,
                Encrypted = 0x00004000,
                Write_Through = 0x80000000,
                Overlapped = 0x40000000,
                NoBuffering = 0x20000000,
                RandomAccess = 0x10000000,
                SequentialScan = 0x08000000,
                DeleteOnClose = 0x04000000,
                BackupSemantics = 0x02000000,
                PosixSemantics = 0x01000000,
                OpenReparsePoint = 0x00200000,
                OpenNoRecall = 0x00100000,
                FirstPipeInstance = 0x00080000
            }
            

            public const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;
            public const int PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY = 0x00020007;

            public const int STARTF_USESTDHANDLES = 0x00000100;
            public const int STARTF_USESHOWWINDOW = 0x00000001;
            public const short SW_HIDE = 0x0000;

            public const long BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000;

        }

        internal class Advapi
        {
            [DllImport("advapi32.dll")]
            public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, LogonType dwlogonType, LogonUserProvider dwlogonProvider, out IntPtr phToken);

            [DllImport("advapi32.dll")]
            public static extern bool RevertToSelf();

            [DllImport("Advapi32.dll")]
            public static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, ref SECURITY_ATTRIBUTES lpTokenAttributes, SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, TOKEN_TYPE TokenType, out IntPtr phNewToken);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool CreateProcessWithLogonW(String userName, String domain, String password, LogonFlags logonFlags, String applicationName, String commandLine, CreationFlags creationFlags, UInt32 environment, String currentDirectory,
                ref StartupInfo startupInfo, out ProcessInformation processInformation);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool CreateProcessAsUserW(IntPtr hToken, string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags,
            IntPtr lpEnvironment, string lpCurrentDirectory, ref StartupInfo lpStartupInfo, out ProcessInformation lpProcessInformation);

            [DllImport("advapi32.dll", EntryPoint = "SetSecurityInfo", CallingConvention = CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern uint SetSecurityInfoByHandle(SafeHandle handle, uint objectType, SECURITY_INFORMATION securityInformation, byte[] owner, byte[] group, byte[] dacl, byte[] sacl);

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern Boolean LookupPrivilegeValue(String lpSystemName, String lpName, ref WinNT._LUID luid);

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern Boolean AdjustTokenPrivileges(IntPtr TokenHandle, Boolean DisableAllPrivileges, ref WinNT._TOKEN_PRIVILEGES NewState, UInt32 BufferLengthInBytes, ref WinNT._TOKEN_PRIVILEGES PreviousState, out UInt32 ReturnLengthInBytes);

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool GetTokenInformation(IntPtr TokenHandle,TOKEN_INFORMATION_CLASS TokenInformationClass,IntPtr TokenInformation,int TokenInformationLength,out int ReturnLength);

            //dll import for PrivilegeCheck
            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool PrivilegeCheck(IntPtr ClientToken, ref WinNT._PRIVILEGE_SET RequiredPrivileges, out bool pfResult);

            //dll import for lookupPrivilegeName
            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool LookupPrivilegeName(String lpSystemName, IntPtr lpLuid, StringBuilder lpName, ref int cchName);

            [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

            [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern Boolean QueryServiceConfig(IntPtr hService, IntPtr intPtrQueryConfig, UInt32 cbBufSize, out UInt32 pcbBytesNeeded);

            [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ChangeServiceConfigA(IntPtr hService, uint dwServiceType, uint dwStartType, int dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, string lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword, string lpDisplayName);

            [DllImport("advapi32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool StartService(IntPtr hService, int dwNumServiceArgs, string[] lpServiceArgVectors);

            [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
            public static extern int CloseServiceHandle(IntPtr hSCObject);

            [DllImport("advapi32.dll", EntryPoint = "OpenProcessToken")]
            public static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

            //create service
            [DllImport("advapi32.dll", EntryPoint = "CreateServiceA", SetLastError = true)]
            public static extern IntPtr CreateService(IntPtr SC_HANDLE, string lpSvcName, string lpDisplayName, uint dwDesiredAccess, uint dwServiceType, uint dwStartType, uint dwErrorControl, string lpPathName, string lpLoadOrderGroup, string lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword);

            //copy file
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern bool CopyFile(string src, string dst, bool failIfExists);

            public static uint SC_MANAGER_ALL_ACCESS = 0xF003F;
            public static uint SERVICE_ALL_ACCESS = 0xF01FF;
            public static uint SERVICE_DEMAND_START = 0x3;
            public static uint SERVICE_NO_CHANGE = 0xffffffff;

            [StructLayout(LayoutKind.Sequential)]
            public class QUERY_SERVICE_CONFIG
            {
                [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
                public UInt32 dwServiceType;
                [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
                public UInt32 dwStartType;
                [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
                public UInt32 dwErrorControl;
                [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
                public String lpBinaryPathName;
                [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
                public String lpLoadOrderGroup;
                [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
                public UInt32 dwTagID;
                [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
                public String lpDependencies;
                [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
                public String lpServiceStartName;
                [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
                public String lpDisplayName;
            };

            // service type 
            public const uint SERVICE_KERNEL_DRIVER = 0x00000001;
            public const uint SERVICE_FILE_SYSTEM_DRIVER = 0x00000002;
            public const uint SERVICE_WIN32_OWN_PROCESS = 0x00000010;
            public const uint SERVICE_WIN32_SHARE_PROCESS = 0x00000020;
            public const uint SERVICE_INTERACTIVE_PROCESS = 0x00000100;

            // start type
            public const uint SERVICE_BOOT_START = 0x00000000;
            public const uint SERVICE_SYSTEM_START = 0x00000001;
            public const uint SERVICE_AUTO_START = 0x00000002;
            public const uint SERVICE_DISABLED = 0x00000004;

            // error control type
            public const uint SERVICE_ERROR_IGNORE = 0x00000000;
            public const uint SERVICE_ERROR_NORMAL = 0x00000001;
            public const uint SERVICE_ERROR_SEVERE = 0x00000002;
            public const uint SERVICE_ERROR_CRITICAL = 0x00000003;

            public enum TOKEN_INFORMATION_CLASS
            {
                TokenUser = 1,
                TokenGroups,
                TokenPrivileges,
                TokenOwner,
                TokenPrimaryGroup,
                TokenDefaultDacl,
                TokenSource,
                TokenType,
                TokenImpersonationLevel,
                TokenStatistics,
                TokenRestrictedSids,
                TokenSessionId,
                TokenGroupsAndPrivileges,
                TokenSessionReference,
                TokenSandBoxInert,
                TokenAuditPolicy,
                TokenOrigin,
                TokenElevationType,
                TokenLinkedToken,
                TokenElevation,
                TokenHasRestrictions,
                TokenAccessInformation,
                TokenVirtualizationAllowed,
                TokenVirtualizationEnabled,
                TokenIntegrityLevel,
                TokenUIAccess,
                TokenMandatoryPolicy,
                TokenLogonSid,
                MaxTokenInfoClass
            }

            [Flags]
            public enum LuidAttributes : uint
            {
                DISABLED = 0x00000000,
                SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001,
                SE_PRIVILEGE_ENABLED = 0x00000002,
                SE_PRIVILEGE_REMOVED = 0x00000004,
                SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000
            }            

            public enum LogonType
            {
                LOGON32_LOGON_INTERACTIVE = 2,
                LOGON32_LOGON_NETWORK = 3,
                LOGON32_LOGON_BATCH = 4,
                LOGON32_LOGON_SERVICE = 5,
                LOGON32_LOGON_UNLOCK = 7,
                LOGON32_LOGON_NETWORK_CLEARTEXT = 8,
                LOGON32_LOGON_NEW_CREDENTIALS = 9
            }

            public enum LogonUserProvider
            {
                LOGON32_PROVIDER_DEFAULT = 0,
                LOGON32_PROVIDER_WINNT35 = 1,
                LOGON32_PROVIDER_WINNT40 = 2,
                LOGON32_PROVIDER_WINNT50 = 3,
                LOGON32_PROVIDER_VIRTUAL = 4,
            }

            public enum TOKEN_TYPE
            {
                TokenPrimary = 1,
                TokenImpersonation
            }
            public enum SECURITY_IMPERSONATION_LEVEL
            {
                SecurityAnonymous,
                SecurityIdentification,
                SecurityImpersonation,
                SecurityDelegation
            }

            [Flags]
            public enum CreationFlags
            {
                CREATE_SUSPENDED = 0x00000004,
                CREATE_NEW_CONSOLE = 0x00000010,
                CREATE_NEW_PROCESS_GROUP = 0x00000200,
                CREATE_UNICODE_ENVIRONMENT = 0x00000400,
                CREATE_SEPARATE_WOW_VDM = 0x00000800,
                CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            }

            [Flags]
            public enum LogonFlags
            {
                LOGON_WITH_PROFILE = 0x00000001,
                LOGON_NETCREDENTIALS_ONLY = 0x00000002
            }


            public const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
            public const UInt32 STANDARD_RIGHTS_READ = 0x00020000;
            public const UInt32 TOKEN_ASSIGN_PRIMARY = 0x0001;
            public const UInt32 TOKEN_DUPLICATE = 0x0002;
            public const UInt32 TOKEN_IMPERSONATE = 0x0004;
            public const UInt32 TOKEN_QUERY = 0x0008;
            public const UInt32 TOKEN_QUERY_SOURCE = 0x0010;
            public const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
            public const UInt32 TOKEN_ADJUST_GROUPS = 0x0040;
            public const UInt32 TOKEN_ADJUST_DEFAULT = 0x0080;
            public const UInt32 TOKEN_ADJUST_SESSIONID = 0x0100;
            public const UInt32 TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
            public const UInt32 TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
                TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
                TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
                TOKEN_ADJUST_SESSIONID);
            public const UInt32 TOKEN_ALT = (TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY);

            [StructLayout(LayoutKind.Sequential)]
            public struct SECURITY_ATTRIBUTES
            {
                public int nLength;
                public IntPtr lpSecurityDescriptor;
                public int bInheritHandle;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct StartupInfo
            {
                public int cb;
                public String reserved;
                public String desktop;
                public String title;
                public int x;
                public int y;
                public int xSize;
                public int ySize;
                public int xCountChars;
                public int yCountChars;
                public int fillAttribute;
                public int flags;
                public UInt16 showWindow;
                public UInt16 reserved2;
                public byte reserved3;
                public IntPtr stdInput;
                public IntPtr stdOutput;
                public IntPtr stdError;
            }

            public struct ProcessInformation
            {
                public IntPtr process;
                public IntPtr thread;
                public int processId;
                public int threadId;
            }

            [Flags]
            public enum SECURITY_INFORMATION : uint
            {
                OWNER_SECURITY_INFORMATION = 0x00000001,
                GROUP_SECURITY_INFORMATION = 0x00000002,
                DACL_SECURITY_INFORMATION = 0x00000004,
                SACL_SECURITY_INFORMATION = 0x00000008,
                LABEL_SECURITY_INFORMATION = 0x00000010,
                ATTRIBUTE_SECURITY_INFORMATION = 0x00000020,
                SCOPE_SECURITY_INFORMATION = 0x00000040,
                PROCESS_TRUST_LABEL_SECURITY_INFORMATION = 0x00000080,
                BACKUP_SECURITY_INFORMATION = 0x00010000,
                PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000,
                PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,
                UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,
                UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000
            }

        }
        public class WinNT
        {
            public const UInt32 PAGE_NOACCESS = 0x01;
            public const UInt32 PAGE_READONLY = 0x02;
            public const UInt32 PAGE_READWRITE = 0x04;
            public const UInt32 PAGE_WRITECOPY = 0x08;
            public const UInt32 PAGE_EXECUTE = 0x10;
            public const UInt32 PAGE_EXECUTE_READ = 0x20;
            public const UInt32 PAGE_EXECUTE_READWRITE = 0x40;
            public const UInt32 PAGE_EXECUTE_WRITECOPY = 0x80;
            public const UInt32 PAGE_GUARD = 0x100;
            public const UInt32 PAGE_NOCACHE = 0x200;
            public const UInt32 PAGE_WRITECOMBINE = 0x400;
            public const UInt32 PAGE_TARGETS_INVALID = 0x40000000;
            public const UInt32 PAGE_TARGETS_NO_UPDATE = 0x40000000;

            public const UInt32 SEC_COMMIT = 0x08000000;
            public const UInt32 SEC_IMAGE = 0x1000000;
            public const UInt32 SEC_IMAGE_NO_EXECUTE = 0x11000000;
            public const UInt32 SEC_LARGE_PAGES = 0x80000000;
            public const UInt32 SEC_NOCACHE = 0x10000000;
            public const UInt32 SEC_RESERVE = 0x4000000;
            public const UInt32 SEC_WRITECOMBINE = 0x40000000;

            public const UInt32 SE_PRIVILEGE_ENABLED = 0x2;
            public const UInt32 SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x1;
            public const UInt32 SE_PRIVILEGE_REMOVED = 0x4;
            public const UInt32 SE_PRIVILEGE_USED_FOR_ACCESS = 0x3;

            public const UInt64 SE_GROUP_ENABLED = 0x00000004L;
            public const UInt64 SE_GROUP_ENABLED_BY_DEFAULT = 0x00000002L;
            public const UInt64 SE_GROUP_INTEGRITY = 0x00000020L;
            public const UInt32 SE_GROUP_INTEGRITY_32 = 0x00000020;
            public const UInt64 SE_GROUP_INTEGRITY_ENABLED = 0x00000040L;
            public const UInt64 SE_GROUP_LOGON_ID = 0xC0000000L;
            public const UInt64 SE_GROUP_MANDATORY = 0x00000001L;
            public const UInt64 SE_GROUP_OWNER = 0x00000008L;
            public const UInt64 SE_GROUP_RESOURCE = 0x20000000L;
            public const UInt64 SE_GROUP_USE_FOR_DENY_ONLY = 0x00000010L;

            public enum _SECURITY_IMPERSONATION_LEVEL
            {
                SecurityAnonymous,
                SecurityIdentification,
                SecurityImpersonation,
                SecurityDelegation
            }

            public enum TOKEN_TYPE
            {
                TokenPrimary = 1,
                TokenImpersonation
            }

            public enum _TOKEN_ELEVATION_TYPE
            {
                TokenElevationTypeDefault = 1,
                TokenElevationTypeFull,
                TokenElevationTypeLimited
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct _MEMORY_BASIC_INFORMATION32
            {
                public UInt32 BaseAddress;
                public UInt32 AllocationBase;
                public UInt32 AllocationProtect;
                public UInt32 RegionSize;
                public UInt32 State;
                public UInt32 Protect;
                public UInt32 Type;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct _MEMORY_BASIC_INFORMATION64
            {
                public UInt64 BaseAddress;
                public UInt64 AllocationBase;
                public UInt32 AllocationProtect;
                public UInt32 __alignment1;
                public UInt64 RegionSize;
                public UInt32 State;
                public UInt32 Protect;
                public UInt32 Type;
                public UInt32 __alignment2;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct _LUID_AND_ATTRIBUTES
            {
                public _LUID Luid;
                public UInt32 Attributes;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct _LUID
            {
                public UInt32 LowPart;
                public Int32 HighPart;

                public _LUID(UInt64 value)
                {
                    LowPart = (UInt32)(value & 0xffffffffL);
                    HighPart = (Int32)(value >> 32);
                }

                public _LUID(_LUID value)
                {
                    LowPart = value.LowPart;
                    HighPart = value.HighPart;
                }

                public _LUID(string value)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^0x[0-9A-Fa-f]+$"))
                    {
                        // if the passed _LUID string is of form 0xABC123
                        UInt64 uintVal = Convert.ToUInt64(value, 16);
                        LowPart = (UInt32)(uintVal & 0xffffffffL);
                        HighPart = (Int32)(uintVal >> 32);
                    }
                    else if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d+$"))
                    {
                        // if the passed _LUID string is a decimal form
                        UInt64 uintVal = UInt64.Parse(value);
                        LowPart = (UInt32)(uintVal & 0xffffffffL);
                        HighPart = (Int32)(uintVal >> 32);
                    }
                    else
                    {
                        System.ArgumentException argEx = new System.ArgumentException("Passed _LUID string value is not in a hex or decimal form", value);
                        throw argEx;
                    }
                }

                public override int GetHashCode()
                {
                    UInt64 Value = ((UInt64)this.HighPart << 32) + this.LowPart;
                    return Value.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    return obj is _LUID && (((ulong)this) == (_LUID)obj);
                }

                public override string ToString()
                {
                    UInt64 Value = ((UInt64)this.HighPart << 32) + this.LowPart;
                    return String.Format("0x{0:x}", (ulong)Value);
                }

                public static bool operator ==(_LUID x, _LUID y)
                {
                    return (((ulong)x) == ((ulong)y));
                }

                public static bool operator !=(_LUID x, _LUID y)
                {
                    return (((ulong)x) != ((ulong)y));
                }

                public static implicit operator ulong(_LUID luid)
                {
                    // enable casting to a ulong
                    UInt64 Value = ((UInt64)luid.HighPart << 32);
                    return Value + luid.LowPart;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct _TOKEN_STATISTICS
            {
                public _LUID TokenId;
                public _LUID AuthenticationId;
                public UInt64 ExpirationTime;
                public TOKEN_TYPE TokenType;
                public _SECURITY_IMPERSONATION_LEVEL ImpersonationLevel;
                public UInt32 DynamicCharged;
                public UInt32 DynamicAvailable;
                public UInt32 GroupCount;
                public UInt32 PrivilegeCount;
                public _LUID ModifiedId;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct _TOKEN_PRIVILEGES
            {
                public UInt32 PrivilegeCount;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 35)]
                public _LUID_AND_ATTRIBUTES[] Privileges;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_GROUPS_AND_PRIVILEGES
            {
                public uint SidCount;
                public uint SidLength;
                public IntPtr Sids;
                public uint RestrictedSidCount;
                public uint RestrictedSidLength;
                public IntPtr RestrictedSids;
                public uint PrivilegeCount;
                public uint PrivilegeLength;
                public IntPtr Privileges;
                public _LUID AuthenticationID;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct _TOKEN_MANDATORY_LABEL
            {
                public _SID_AND_ATTRIBUTES Label;
            }

            public struct _SID
            {
                public byte Revision;
                public byte SubAuthorityCount;
                public WinNT._SID_IDENTIFIER_AUTHORITY IdentifierAuthority;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
                public ulong[] SubAuthority;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct _SID_IDENTIFIER_AUTHORITY
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.I1)]
                public byte[] Value;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct _SID_AND_ATTRIBUTES
            {
                public IntPtr Sid;
                public UInt32 Attributes;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct _PRIVILEGE_SET
            {
                public UInt32 PrivilegeCount;
                public UInt32 Control;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
                public _LUID_AND_ATTRIBUTES[] Privilege;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct _TOKEN_USER
            {
                public _SID_AND_ATTRIBUTES User;
            }

            public enum _SID_NAME_USE
            {
                SidTypeUser = 1,
                SidTypeGroup,
                SidTypeDomain,
                SidTypeAlias,
                SidTypeWellKnownGroup,
                SidTypeDeletedAccount,
                SidTypeInvalid,
                SidTypeUnknown,
                SidTypeComputer,
                SidTypeLabel
            }

            public enum _TOKEN_INFORMATION_CLASS
            {
                TokenUser = 1,
                TokenGroups,
                TokenPrivileges,
                TokenOwner,
                TokenPrimaryGroup,
                TokenDefaultDacl,
                TokenSource,
                TokenType,
                TokenImpersonationLevel,
                TokenStatistics,
                TokenRestrictedSids,
                TokenSessionId,
                TokenGroupsAndPrivileges,
                TokenSessionReference,
                TokenSandBoxInert,
                TokenAuditPolicy,
                TokenOrigin,
                TokenElevationType,
                TokenLinkedToken,
                TokenElevation,
                TokenHasRestrictions,
                TokenAccessInformation,
                TokenVirtualizationAllowed,
                TokenVirtualizationEnabled,
                TokenIntegrityLevel,
                TokenUIAccess,
                TokenMandatoryPolicy,
                TokenLogonSid,
                TokenIsAppContainer,
                TokenCapabilities,
                TokenAppContainerSid,
                TokenAppContainerNumber,
                TokenUserClaimAttributes,
                TokenDeviceClaimAttributes,
                TokenRestrictedUserClaimAttributes,
                TokenRestrictedDeviceClaimAttributes,
                TokenDeviceGroups,
                TokenRestrictedDeviceGroups,
                TokenSecurityAttributes,
                TokenIsRestricted,
                MaxTokenInfoClass
            }

            // http://www.pinvoke.net/default.aspx/Enums.ACCESS_MASK
            [Flags]
            public enum ACCESS_MASK : uint
            {
                DELETE = 0x00010000,
                READ_CONTROL = 0x00020000,
                WRITE_DAC = 0x00040000,
                WRITE_OWNER = 0x00080000,
                SYNCHRONIZE = 0x00100000,
                STANDARD_RIGHTS_REQUIRED = 0x000F0000,
                STANDARD_RIGHTS_READ = 0x00020000,
                STANDARD_RIGHTS_WRITE = 0x00020000,
                STANDARD_RIGHTS_EXECUTE = 0x00020000,
                STANDARD_RIGHTS_ALL = 0x001F0000,
                SPECIFIC_RIGHTS_ALL = 0x0000FFF,
                ACCESS_SYSTEM_SECURITY = 0x01000000,
                MAXIMUM_ALLOWED = 0x02000000,
                GENERIC_READ = 0x80000000,
                GENERIC_WRITE = 0x40000000,
                GENERIC_EXECUTE = 0x20000000,
                GENERIC_ALL = 0x10000000,
                DESKTOP_READOBJECTS = 0x00000001,
                DESKTOP_CREATEWINDOW = 0x00000002,
                DESKTOP_CREATEMENU = 0x00000004,
                DESKTOP_HOOKCONTROL = 0x00000008,
                DESKTOP_JOURNALRECORD = 0x00000010,
                DESKTOP_JOURNALPLAYBACK = 0x00000020,
                DESKTOP_ENUMERATE = 0x00000040,
                DESKTOP_WRITEOBJECTS = 0x00000080,
                DESKTOP_SWITCHDESKTOP = 0x00000100,
                WINSTA_ENUMDESKTOPS = 0x00000001,
                WINSTA_READATTRIBUTES = 0x00000002,
                WINSTA_ACCESSCLIPBOARD = 0x00000004,
                WINSTA_CREATEDESKTOP = 0x00000008,
                WINSTA_WRITEATTRIBUTES = 0x00000010,
                WINSTA_ACCESSGLOBALATOMS = 0x00000020,
                WINSTA_EXITWINDOWS = 0x00000040,
                WINSTA_ENUMERATE = 0x00000100,
                WINSTA_READSCREEN = 0x00000200,
                WINSTA_ALL_ACCESS = 0x0000037F,

                SECTION_ALL_ACCESS = 0x10000000,
                SECTION_QUERY = 0x0001,
                SECTION_MAP_WRITE = 0x0002,
                SECTION_MAP_READ = 0x0004,
                SECTION_MAP_EXECUTE = 0x0008,
                SECTION_EXTEND_SIZE = 0x0010
            };
        }
    }
}
