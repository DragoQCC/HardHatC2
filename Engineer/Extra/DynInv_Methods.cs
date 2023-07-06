// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Runtime.InteropServices;
// using System.Text;
// using System.Threading.Tasks;
// using DynamicEngLoading;
// using static DynamicEngLoading.h_DynInv;
//
// namespace Engineer.Extra
// {
//     //placed these here to help keep the other "header" file as clean as possible 
//     public class h_DynInv_Methods
//     {
//         public class NtFuncWrapper
//         {
//             public static NTSTATUS NtCreateThreadEx(ref IntPtr threadHandle,
//                 Win32.WinNT.ACCESS_MASK desiredAccess, IntPtr objectAttributes, IntPtr processHandle,
//                 IntPtr startAddress, IntPtr parameter, bool createSuspended, int stackZeroBits, int sizeOfStack,
//                 int maximumStackSize, IntPtr attributeList)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     threadHandle, desiredAccess, objectAttributes, processHandle, startAddress, parameter,
//                     createSuspended, stackZeroBits,
//                     sizeOfStack, maximumStackSize, attributeList
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtCreateThreadEx",
//                     typeof(NT_DELEGATES.NtCreateThreadEx), ref funcargs);
//
//                 // Update the modified variables
//                 threadHandle = (IntPtr)funcargs[0];
//
//                 return retValue;
//             }
//
//             public static NTSTATUS RtlCreateUserThread(IntPtr Process, IntPtr ThreadSecurityDescriptor, bool CreateSuspended, IntPtr ZeroBits, IntPtr MaximumStackSize, IntPtr CommittedStackSize, IntPtr StartAddress, IntPtr Parameter, ref IntPtr Thread, IntPtr ClientId)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     Process, ThreadSecurityDescriptor, CreateSuspended, ZeroBits,
//                     MaximumStackSize, CommittedStackSize, StartAddress, Parameter,
//                     Thread, ClientId
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"RtlCreateUserThread",
//                     typeof(NT_DELEGATES.RtlCreateUserThread), ref funcargs);
//
//                 // Update the modified variables
//                 Thread = (IntPtr)funcargs[8];
//
//                 return retValue;
//             }
//
//             public static NTSTATUS NtCreateSection(ref IntPtr SectionHandle, uint DesiredAccess,
//                 IntPtr ObjectAttributes, ref ulong MaximumSize, uint SectionPageProtection, uint AllocationAttributes,
//                 IntPtr FileHandle)
//             {
//
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     SectionHandle, DesiredAccess, ObjectAttributes, MaximumSize, SectionPageProtection,
//                     AllocationAttributes, FileHandle
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtCreateSection",
//                     typeof(NT_DELEGATES.NtCreateSection), ref funcargs);
//                 if (retValue != NTSTATUS.Success)
//                 {
//                     throw new InvalidOperationException("Unable to create section, " + retValue);
//                 }
//
//                 // Update the modified variables
//                 SectionHandle = (IntPtr)funcargs[0];
//                 MaximumSize = (ulong)funcargs[3];
//
//                 return retValue;
//             }
//
//             public static NTSTATUS NtUnmapViewOfSection(IntPtr hProc, IntPtr baseAddr)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     hProc, baseAddr
//                 };
//
//                 NTSTATUS result = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtUnmapViewOfSection",
//                     typeof(NT_DELEGATES.NtUnmapViewOfSection), ref funcargs);
//
//                 return result;
//             }
//
//             public static NTSTATUS NtMapViewOfSection(IntPtr SectionHandle, IntPtr ProcessHandle,
//                 ref IntPtr BaseAddress, IntPtr ZeroBits, IntPtr CommitSize, IntPtr SectionOffset, ref ulong ViewSize,
//                 uint InheritDisposition, uint AllocationType, uint Win32Protect)
//             {
//
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     SectionHandle, ProcessHandle, BaseAddress, ZeroBits, CommitSize, SectionOffset, ViewSize,
//                     InheritDisposition, AllocationType,
//                     Win32Protect
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtMapViewOfSection",
//                     typeof(NT_DELEGATES.NtMapViewOfSection), ref funcargs);
//                 if (retValue != NTSTATUS.Success && retValue != NTSTATUS.ImageNotAtBase)
//                 {
//                     throw new InvalidOperationException("Unable to map view of section, " + retValue);
//                 }
//
//                 // Update the modified variables.
//                 BaseAddress = (IntPtr)funcargs[2];
//                 ViewSize = (ulong)funcargs[6];
//
//                 return retValue;
//             }
//
//             public static void RtlInitUnicodeString(ref UNICODE_STRING DestinationString,
//                 [MarshalAs(UnmanagedType.LPWStr)] string SourceString)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     DestinationString, SourceString
//                 };
//
//                 DynInv.DynamicAPIInvoke(@"ntdll.dll", @"RtlInitUnicodeString",
//                     typeof(NT_DELEGATES.RtlInitUnicodeString), ref funcargs);
//
//                 // Update the modified variables
//                 DestinationString = (UNICODE_STRING)funcargs[0];
//             }
//
//             public static NTSTATUS LdrLoadDll(IntPtr PathToFile, UInt32 dwFlags, ref UNICODE_STRING ModuleFileName,
//                 ref IntPtr ModuleHandle)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     PathToFile, dwFlags, ModuleFileName, ModuleHandle
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"LdrLoadDll",
//                     typeof(NT_DELEGATES.LdrLoadDll), ref funcargs);
//
//                 // Update the modified variables
//                 ModuleHandle = (IntPtr)funcargs[3];
//
//                 return retValue;
//             }
//
//             public static void RtlZeroMemory(IntPtr Destination, int Length)
//             {
//                 try
//                 {
//                     // Craft an array for the arguments
//                     object[] funcargs =
//                     {
//                         Destination, Length
//                     };
//                     DynInv.DynamicAPIInvoke(@"ntdll.dll", @"RtlZeroMemory", typeof(NT_DELEGATES.RtlZeroMemory), ref funcargs);
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine($"error in RtlZeroMemory: {ex.Message}");
//                     Console.WriteLine(Ker32FuncWrapper.GetLastError());
//                 }
//             }
//
//             public static NTSTATUS NtQueryInformationProcess(IntPtr hProcess, PROCESSINFOCLASS processInfoClass, out IntPtr pProcInfo)
//             {
//                 try
//                 {
//                     int processInformationLength;
//                     UInt32 RetLen = 0;
//
//                     switch (processInfoClass)
//                     {
//                         case PROCESSINFOCLASS.ProcessWow64Information:
//                             pProcInfo = Marshal.AllocHGlobal(IntPtr.Size);
//                             RtlZeroMemory(pProcInfo, IntPtr.Size);
//                             processInformationLength = IntPtr.Size;
//                             break;
//                         case PROCESSINFOCLASS.ProcessBasicInformation:
//                             PROCESS_BASIC_INFORMATION PBI = new PROCESS_BASIC_INFORMATION();
//                             pProcInfo = Marshal.AllocHGlobal(Marshal.SizeOf(PBI));
//                             RtlZeroMemory(pProcInfo, Marshal.SizeOf(PBI));
//                             Marshal.StructureToPtr(PBI, pProcInfo, true);
//                             processInformationLength = Marshal.SizeOf(PBI);
//                             break;
//                         default:
//                             throw new InvalidOperationException($"Invalid ProcessInfoClass: {processInfoClass}");
//                     }
//
//                     object[] funcargs =
//                     {
//                         hProcess, processInfoClass, pProcInfo, processInformationLength, RetLen
//                     };
//
//                     NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtQueryInformationProcess",
//                         typeof(NT_DELEGATES.NtQueryInformationProcess), ref funcargs);
//                     if (retValue != NTSTATUS.Success)
//                     {
//                         throw new UnauthorizedAccessException("Access is denied.");
//                     }
//
//                     // Update the modified variables
//                     pProcInfo = (IntPtr)funcargs[2];
//
//                     return retValue;
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine($"error in NtQueryInformationProcess {ex.Message}");
//                     Console.WriteLine(Ker32FuncWrapper.GetLastError());
//                     pProcInfo = IntPtr.Zero;
//                     return NTSTATUS.AccessDenied;
//                 }
//                
//             }
//
//             public static bool NtQueryInformationProcessWow64Information(IntPtr hProcess)
//             {
//                 NTSTATUS retValue = NtQueryInformationProcess(hProcess, PROCESSINFOCLASS.ProcessWow64Information,
//                     out IntPtr pProcInfo);
//                 if (retValue != NTSTATUS.Success)
//                 {
//                     throw new UnauthorizedAccessException("Access is denied.");
//                 }
//
//                 if (Marshal.ReadIntPtr(pProcInfo) == IntPtr.Zero)
//                 {
//                     return false;
//                 }
//
//                 return true;
//             }
//
//             public static PROCESS_BASIC_INFORMATION NtQueryInformationProcessBasicInformation(IntPtr hProcess)
//             {
//                 try
//                 {
//                     NTSTATUS retValue = NtQueryInformationProcess(hProcess, PROCESSINFOCLASS.ProcessBasicInformation, out IntPtr pProcInfo);
//                     if (retValue != NTSTATUS.Success)
//                     {
//                         throw new UnauthorizedAccessException("Access is denied.");
//                     }
//
//                     return (PROCESS_BASIC_INFORMATION)Marshal.PtrToStructure(pProcInfo, typeof(PROCESS_BASIC_INFORMATION));
//
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine($"error in NtQueryInformationProcessBasicInformation: {ex.Message}");
//                     Console.WriteLine(Ker32FuncWrapper.GetLastError());
//                     return new PROCESS_BASIC_INFORMATION();
//                 }
//                 
//             }
//
//             public static IntPtr NtOpenProcess(UInt32 ProcessId,Win32.Kernel32.ProcessAccessFlags DesiredAccess)
//             {
//                 // Create OBJECT_ATTRIBUTES & CLIENT_ID ref's
//                 IntPtr ProcessHandle = IntPtr.Zero;
//                 OBJECT_ATTRIBUTES oa = new OBJECT_ATTRIBUTES();
//                 CLIENT_ID ci = new CLIENT_ID();
//                 ci.UniqueProcess = (IntPtr)ProcessId;
//
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     ProcessHandle, DesiredAccess, oa, ci
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtOpenProcess", typeof(NT_DELEGATES.NtOpenProcess), ref funcargs);
//                 if (retValue != NTSTATUS.Success && retValue == NTSTATUS.InvalidCid)
//                 {
//                     throw new InvalidOperationException("An invalid client ID was specified.");
//                 }
//
//                 if (retValue != NTSTATUS.Success)
//                 {
//                     throw new UnauthorizedAccessException("Access is denied.");
//                 }
//
//                 // Update the modified variables
//                 ProcessHandle = (IntPtr)funcargs[0];
//
//                 return ProcessHandle;
//             }
//
//             public static void NtQueueApcThread(IntPtr ThreadHandle, IntPtr ApcRoutine, IntPtr ApcArgument1, IntPtr ApcArgument2, IntPtr ApcArgument3)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     ThreadHandle, ApcRoutine, ApcArgument1, ApcArgument2, ApcArgument3
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtQueueApcThread",
//                     typeof(NT_DELEGATES.NtQueueApcThread), ref funcargs);
//                 if (retValue != NTSTATUS.Success)
//                 {
//                     throw new InvalidOperationException("Unable to queue APC, " + retValue);
//                 }
//             }
//
//             public static IntPtr NtOpenThread(int TID, Win32.Kernel32.ThreadAccess DesiredAccess)
//             {
//                 // Create OBJECT_ATTRIBUTES & CLIENT_ID ref's
//                 IntPtr ThreadHandle = IntPtr.Zero;
//                 OBJECT_ATTRIBUTES oa = new OBJECT_ATTRIBUTES();
//                 CLIENT_ID ci = new CLIENT_ID();
//                 ci.UniqueThread = (IntPtr)TID;
//
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     ThreadHandle, DesiredAccess, oa, ci
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtOpenThread", typeof(NT_DELEGATES.NtOpenProcess), ref funcargs);
//                 if (retValue != NTSTATUS.Success && retValue == NTSTATUS.InvalidCid)
//                 {
//                     throw new InvalidOperationException("An invalid client ID was specified.");
//                 }
//
//                 if (retValue != NTSTATUS.Success)
//                 {
//                     throw new UnauthorizedAccessException("Access is denied.");
//                 }
//
//                 // Update the modified variables
//                 ThreadHandle = (IntPtr)funcargs[0];
//
//                 return ThreadHandle;
//             }
//
//             public static IntPtr NtAllocateVirtualMemory(IntPtr ProcessHandle, ref IntPtr BaseAddress, IntPtr ZeroBits,ref IntPtr RegionSize, Win32.Kernel32.AllocationType AllocationType, UInt32 Protect)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     ProcessHandle, BaseAddress, ZeroBits, RegionSize, AllocationType, Protect
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtAllocateVirtualMemory",
//                     typeof(NT_DELEGATES.NtAllocateVirtualMemory), ref funcargs);
//                 if (retValue == NTSTATUS.AccessDenied)
//                 {
//                     // STATUS_ACCESS_DENIED
//                     throw new UnauthorizedAccessException("Access is denied.");
//                 }
//
//                 if (retValue == NTSTATUS.AlreadyCommitted)
//                 {
//                     // STATUS_ALREADY_COMMITTED
//                     throw new InvalidOperationException("The specified address range is already committed.");
//                 }
//
//                 if (retValue == NTSTATUS.CommitmentLimit)
//                 {
//                     // STATUS_COMMITMENT_LIMIT
//                     throw new InvalidOperationException("Your system is low on virtual memory.");
//                 }
//
//                 if (retValue == NTSTATUS.ConflictingAddresses)
//                 {
//                     // STATUS_CONFLICTING_ADDRESSES
//                     throw new InvalidOperationException(
//                         "The specified address range conflicts with the address space.");
//                 }
//
//                 if (retValue == NTSTATUS.InsufficientResources)
//                 {
//                     // STATUS_INSUFFICIENT_RESOURCES
//                     throw new InvalidOperationException(
//                         "Insufficient system resources exist to complete the API call.");
//                 }
//
//                 if (retValue == NTSTATUS.InvalidHandle)
//                 {
//                     // STATUS_INVALID_HANDLE
//                     throw new InvalidOperationException("An invalid HANDLE was specified.");
//                 }
//
//                 if (retValue == NTSTATUS.InvalidPageProtection)
//                 {
//                     // STATUS_INVALID_PAGE_PROTECTION
//                     throw new InvalidOperationException("The specified page protection was not valid.");
//                 }
//
//                 if (retValue == NTSTATUS.NoMemory)
//                 {
//                     // STATUS_NO_MEMORY
//                     throw new InvalidOperationException(
//                         "Not enough virtual memory or paging file quota is available to complete the specified operation.");
//                 }
//
//                 if (retValue == NTSTATUS.ObjectTypeMismatch)
//                 {
//                     // STATUS_OBJECT_TYPE_MISMATCH
//                     throw new InvalidOperationException(
//                         "There is a mismatch between the type of object that is required by the requested operation and the type of object that is specified in the request.");
//                 }
//
//                 if (retValue != NTSTATUS.Success)
//                 {
//                     // STATUS_PROCESS_IS_TERMINATING == 0xC000010A
//                     throw new InvalidOperationException(
//                         "An attempt was made to duplicate an object handle into or out of an exiting process.");
//                 }
//
//                 BaseAddress = (IntPtr)funcargs[1];
//                 return BaseAddress;
//             }
//
//             public static void NtFreeVirtualMemory(IntPtr ProcessHandle, ref IntPtr BaseAddress, ref IntPtr RegionSize, Win32.Kernel32.AllocationType FreeType)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     ProcessHandle, BaseAddress, RegionSize, FreeType
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtFreeVirtualMemory",
//                     typeof(NT_DELEGATES.NtFreeVirtualMemory), ref funcargs);
//                 if (retValue == NTSTATUS.AccessDenied)
//                 {
//                     // STATUS_ACCESS_DENIED
//                     throw new UnauthorizedAccessException("Access is denied.");
//                 }
//
//                 if (retValue == NTSTATUS.InvalidHandle)
//                 {
//                     // STATUS_INVALID_HANDLE
//                     throw new InvalidOperationException("An invalid HANDLE was specified.");
//                 }
//
//                 if (retValue != NTSTATUS.Success)
//                 {
//                     // STATUS_OBJECT_TYPE_MISMATCH == 0xC0000024
//                     throw new InvalidOperationException(
//                         "There is a mismatch between the type of object that is required by the requested operation and the type of object that is specified in the request.");
//                 }
//             }
//
//             public static string GetFilenameFromMemoryPointer(IntPtr hProc, IntPtr pMem)
//             {
//                 // Alloc buffer for result struct
//                 IntPtr pBase = IntPtr.Zero;
//                 IntPtr RegionSize = (IntPtr)0x500;
//                 IntPtr pAlloc = NtAllocateVirtualMemory(hProc, ref pBase, IntPtr.Zero, ref RegionSize,
//                     Win32.Kernel32.AllocationType.Commit | Win32.Kernel32.AllocationType.Reserve,
//                     Win32.WinNT.PAGE_READWRITE);
//
//                 // Prepare NtQueryVirtualMemory parameters
//                 MEMORYINFOCLASS memoryInfoClass = MEMORYINFOCLASS.MemorySectionName;
//                 UInt32 MemoryInformationLength = 0x500;
//                 UInt32 Retlen = 0;
//
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     hProc, pMem, memoryInfoClass, pAlloc, MemoryInformationLength, Retlen
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtQueryVirtualMemory",
//                     typeof(NT_DELEGATES.NtQueryVirtualMemory), ref funcargs);
//
//                 string FilePath = string.Empty;
//                 if (retValue == NTSTATUS.Success)
//                 {
//                     UNICODE_STRING sn = (UNICODE_STRING)Marshal.PtrToStructure(pAlloc, typeof(UNICODE_STRING));
//                     FilePath = Marshal.PtrToStringUni(sn.Buffer);
//                 }
//
//                 // Free allocation
//                 NtFreeVirtualMemory(hProc, ref pAlloc, ref RegionSize, Win32.Kernel32.AllocationType.Reserve);
//                 if (retValue == NTSTATUS.AccessDenied)
//                 {
//                     // STATUS_ACCESS_DENIED
//                     throw new UnauthorizedAccessException("Access is denied.");
//                 }
//
//                 if (retValue == NTSTATUS.AccessViolation)
//                 {
//                     // STATUS_ACCESS_VIOLATION
//                     throw new InvalidOperationException("The specified base address is an invalid virtual address.");
//                 }
//
//                 if (retValue == NTSTATUS.InfoLengthMismatch)
//                 {
//                     // STATUS_INFO_LENGTH_MISMATCH
//                     throw new InvalidOperationException(
//                         "The MemoryInformation buffer is larger than MemoryInformationLength.");
//                 }
//
//                 if (retValue == NTSTATUS.InvalidParameter)
//                 {
//                     // STATUS_INVALID_PARAMETER
//                     throw new InvalidOperationException(
//                         "The specified base address is outside the range of accessible addresses.");
//                 }
//
//                 return FilePath;
//             }
//
//             public static UInt32 NtProtectVirtualMemory(IntPtr ProcessHandle, ref IntPtr BaseAddress,
//                 ref IntPtr RegionSize, UInt32 NewProtect)
//             {
//                 // Craft an array for the arguments
//                 UInt32 OldProtect = 0;
//                 object[] funcargs =
//                 {
//                     ProcessHandle, BaseAddress, RegionSize, NewProtect, OldProtect
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtProtectVirtualMemory",
//                     typeof(NT_DELEGATES.NtProtectVirtualMemory), ref funcargs);
//                 if (retValue != NTSTATUS.Success)
//                 {
//                     throw new InvalidOperationException("Failed to change memory protection, " + retValue);
//                 }
//
//                 OldProtect = (UInt32)funcargs[4];
//                 return OldProtect;
//             }
//
//             public static UInt32 NtWriteVirtualMemory(IntPtr ProcessHandle, IntPtr BaseAddress, IntPtr Buffer, UInt32 BufferLength)
//             {
//                 // Craft an array for the arguments
//                 UInt32 BytesWritten = 0;
//                 object[] funcargs =
//                 {
//                     ProcessHandle, BaseAddress, Buffer, BufferLength, BytesWritten
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtWriteVirtualMemory",
//                     typeof(NT_DELEGATES.NtWriteVirtualMemory), ref funcargs);
//                 if (retValue != NTSTATUS.Success)
//                 {
//                     throw new InvalidOperationException("Failed to write memory, " + retValue);
//                 }
//
//                 BytesWritten = (UInt32)funcargs[4];
//                 return BytesWritten;
//             }
//
//             public static IntPtr LdrGetProcedureAddress(IntPtr hModule, IntPtr FunctionName, IntPtr Ordinal,ref IntPtr FunctionAddress)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     hModule, FunctionName, Ordinal, FunctionAddress
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"LdrGetProcedureAddress",
//                     typeof(NT_DELEGATES.LdrGetProcedureAddress), ref funcargs);
//                 if (retValue != NTSTATUS.Success)
//                 {
//                     throw new InvalidOperationException("Failed get procedure address, " + retValue);
//                 }
//
//                 FunctionAddress = (IntPtr)funcargs[3];
//                 return FunctionAddress;
//             }
//
//             public static void RtlGetVersion(ref OSVERSIONINFOEX VersionInformation)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     VersionInformation
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"RtlGetVersion",
//                     typeof(NT_DELEGATES.RtlGetVersion), ref funcargs);
//                 if (retValue != NTSTATUS.Success)
//                 {
//                     throw new InvalidOperationException("Failed get procedure address, " + retValue);
//                 }
//
//                 VersionInformation = (OSVERSIONINFOEX)funcargs[0];
//             }
//
//             public static UInt32 NtReadVirtualMemory(IntPtr ProcessHandle, IntPtr BaseAddress, IntPtr Buffer,
//                 ref UInt32 NumberOfBytesToRead)
//             {
//                 // Craft an array for the arguments
//                 UInt32 NumberOfBytesRead = 0;
//                 object[] funcargs =
//                 {
//                     ProcessHandle, BaseAddress, Buffer, NumberOfBytesToRead, NumberOfBytesRead
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtReadVirtualMemory",
//                     typeof(NT_DELEGATES.NtReadVirtualMemory), ref funcargs);
//                 if (retValue != NTSTATUS.Success)
//                 {
//                     throw new InvalidOperationException("Failed to read memory, " + retValue);
//                 }
//
//                 NumberOfBytesRead = (UInt32)funcargs[4];
//                 return NumberOfBytesRead;
//             }
//
//             public static IntPtr NtOpenFile(ref IntPtr FileHandle, Win32.Kernel32.FileAccessFlags DesiredAccess, ref OBJECT_ATTRIBUTES ObjAttr, ref IO_STATUS_BLOCK IoStatusBlock, Win32.Kernel32.FileShareFlags ShareAccess,Win32.Kernel32.FileOpenFlags OpenOptions)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     FileHandle, DesiredAccess, ObjAttr, IoStatusBlock, ShareAccess, OpenOptions
//                 };
//
//                 NTSTATUS retValue = (NTSTATUS)DynInv.DynamicAPIInvoke(@"ntdll.dll", @"NtOpenFile",
//                     typeof(NT_DELEGATES.NtOpenFile), ref funcargs);
//                 if (retValue != NTSTATUS.Success)
//                 {
//                     throw new InvalidOperationException("Failed to open file, " + retValue);
//                 }
//
//
//                 FileHandle = (IntPtr)funcargs[0];
//                 return FileHandle;
//             }
//
//             //func wrapper for UInt32 NtQueryVirtualMemory(  IntPtr ProcessHandle, IntPtr BaseAddress,  MEMORYINFOCLASS MemoryInformationClass,  IntPtr MemoryInformation,  UInt32 MemoryInformationLength, ref UInt32 ReturnLength)
//             public static uint NtQueryVirtualMemory(IntPtr ProcessHandle, IntPtr BaseAddress,  MEMORYINFOCLASS MemoryInformationClass,  IntPtr MemoryInformation,  UInt32 MemoryInformationLength, ref UInt32 ReturnLength)
//             {
//                 object[] funcargs = {ProcessHandle,BaseAddress,MemoryInformationClass, MemoryInformation, MemoryInformationLength, ReturnLength };
//                 NTSTATUS ret = (NTSTATUS)DynInv.DynamicAPIInvoke("ntdll.dll", @"NtQueryVirtualMemory", typeof(NT_DELEGATES.NtQueryVirtualMemory), ref funcargs);
//                 if (ret != NTSTATUS.Success)
//                 {
//                     throw new InvalidOperationException("Failed to query virtual memory, " + ret);
//                 }
//                 return ReturnLength = (uint)funcargs[5];
//
//             }
//
//             //func wraper for NtResumeThread
//             public static bool NtResumeThread(IntPtr ThreadHandle, out uint SuspendCount)
//             {
//                 SuspendCount = 0;
//                 object[] funcargs = { ThreadHandle, SuspendCount };
//                 NTSTATUS ret = (NTSTATUS)DynInv.DynamicAPIInvoke("ntdll.dll", @"NtResumeThread", typeof(NT_DELEGATES.NtResumeThread), ref funcargs);
//                 SuspendCount = (uint)funcargs[1];
//                 if (ret != NTSTATUS.Success)
//                 {
//                     return false;
//                 }
//                 else
//                 {
//                     return true;
//                 }
//             }
//
//         }
//
//
//         public class Ker32FuncWrapper
//         {
//             public static uint GetCurrentThreadId()
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                 };
//                 uint tid = (uint)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"GetCurrentThreadId",
//                     typeof(KER32_DELEGATES.GetCurrentThreadId), ref funcargs);
//                 return tid;
//             }
//
//             //function wrapper for DynInv of CreateProcessW 
//             public static bool CreateProcessW(string lpApplicationName, string lpCommandLine,ref Win32.WinBase._SECURITY_ATTRIBUTES lpProcessAttributes,ref Win32.WinBase._SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags,
//                 IntPtr lpEnvironment, string lpCurrentDirectory, ref Win32.ProcessThreadsAPI._STARTUPINFOEX lpStartupInfo,out Win32.ProcessThreadsAPI._PROCESS_INFORMATION lpProcessInformation)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles,
//                     dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo,
//                     new Win32.ProcessThreadsAPI._PROCESS_INFORMATION()
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"CreateProcessW",
//                     typeof(KER32_DELEGATES.CreateProcessW), ref funcargs);
//                 lpProcessInformation = (Win32.ProcessThreadsAPI._PROCESS_INFORMATION)funcargs[9];
//                 return retValue;
//             }
//
//             //func wrapper for DynInv of WriteProcessMemory
//             public static bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, uint nSize,ref uint lpNumberOfBytesWritten)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     hProcess, lpBaseAddress, lpBuffer, nSize, lpNumberOfBytesWritten
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"WriteProcessMemory",
//                     typeof(KER32_DELEGATES.WriteProcessMemory), ref funcargs);
//                 lpNumberOfBytesWritten = (uint)funcargs[4];
//                 return retValue;
//             }
//             //func wrapper for DynInv of VirtualProtect
//             public static bool VirtualProtect(IntPtr lpAddress, uint dwSize, Win32.Kernel32.MemoryProtection flNewProtect, out Win32.Kernel32.MemoryProtection lpflOldProtect)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     lpAddress, dwSize, flNewProtect, new Win32.Kernel32.MemoryProtection()
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"VirtualProtect",
//                     typeof(KER32_DELEGATES.VirtualProtect), ref funcargs);
//                 lpflOldProtect = (Win32.Kernel32.MemoryProtection)funcargs[3];
//                 return retValue;
//             }
//             //func wrapper for uint QueueUserAPC(IntPtr pfnAPC, IntPtr hThread, uint dwData);
//             public static uint QueueUserAPC(IntPtr pfnAPC, IntPtr hThread, uint dwData)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     pfnAPC, hThread, dwData
//                 };
//
//                 uint retValue = (uint)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"QueueUserAPC",
//                     typeof(KER32_DELEGATES.QueueUserAPC), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for uint ResumeThread(IntPtr hThread);
//             public static uint ResumeThread(IntPtr hThread)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     hThread
//                 };
//
//                 uint retValue = (uint)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"ResumeThread",
//                     typeof(KER32_DELEGATES.ResumeThread), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr param, uint dwCreationFlags, ref uint lpThreadId);
//             public static IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr param, uint dwCreationFlags, ref uint lpThreadId)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     lpThreadAttributes, dwStackSize, lpStartAddress, param, dwCreationFlags, lpThreadId
//                 };
//
//                 IntPtr retValue = (IntPtr)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"CreateThread",
//                     typeof(KER32_DELEGATES.CreateThread), ref funcargs);
//                 lpThreadId = (uint)funcargs[5];
//                 return retValue;
//             }
//             //func wrapper for IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, ref uint lpThreadId);
//             public static IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, ref uint lpThreadId)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     hProcess, lpThreadAttributes, dwStackSize, lpStartAddress, lpParameter, dwCreationFlags, lpThreadId
//                 };
//
//                 IntPtr retValue = (IntPtr)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"CreateRemoteThread",
//                     typeof(KER32_DELEGATES.CreateRemoteThread), ref funcargs);
//                 lpThreadId = (uint)funcargs[6];
//                 return retValue;
//             }
//             //func wrapper for IntPtr VirtualAlloc(IntPtr lpStartAddr, uint size, uint flAllocationType, uint flProtect);
//             public static IntPtr VirtualAlloc(IntPtr lpStartAddr, uint size, uint flAllocationType, uint flProtect)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     lpStartAddr, size, flAllocationType, flProtect
//                 };
//
//                 IntPtr retValue = (IntPtr)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"VirtualAlloc",
//                     typeof(KER32_DELEGATES.VirtualAlloc), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for  IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect)
//             public static IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     hProcess, lpAddress, dwSize, flAllocationType, flProtect
//                 };
//
//                 IntPtr retValue = (IntPtr)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"VirtualAllocEx",
//                     typeof(KER32_DELEGATES.VirtualAllocEx), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds)
//             public static uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     hHandle, dwMilliseconds
//                 };
//
//                 uint retValue = (uint)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"WaitForSingleObject",
//                     typeof(KER32_DELEGATES.WaitForSingleObject), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for IntPtr LoadLibraryW(string library);
//             public static IntPtr LoadLibraryW(string library)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     library
//                 };
//
//                 IntPtr retValue = (IntPtr)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"LoadLibraryW",
//                     typeof(KER32_DELEGATES.LoadLibraryW), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for GetProcAddress(IntPtr libPtr, string function)
//             public static IntPtr GetProcAddress(IntPtr libPtr, string function)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     libPtr, function
//                 };
//
//                 IntPtr retValue = (IntPtr)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"GetProcAddress",
//                     typeof(KER32_DELEGATES.GetProcAddress), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for bool freeLibrary(IntPtr library)
//             public static bool freeLibrary(IntPtr library)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     library
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"FreeLibrary",
//                     typeof(KER32_DELEGATES.FreeLibrary), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for bool ConvertThreadToFiber(IntPtr lpParameter)
//             public static bool ConvertThreadToFiber(IntPtr lpParameter)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     lpParameter
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"ConvertThreadToFiber",
//                     typeof(KER32_DELEGATES.ConvertThreadToFiber), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for  IntPtr CreateFiber(uint dwStackSize, uint lpStartAddress, IntPtr lpParameter)
//             public static IntPtr CreateFiber(uint dwStackSize, uint lpStartAddress, IntPtr lpParameter)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     dwStackSize, lpStartAddress, lpParameter
//                 };
//
//                 IntPtr retValue = (IntPtr)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"CreateFiber",
//                     typeof(KER32_DELEGATES.CreateFiber), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for uint SwitchToFiber(IntPtr lpFiber)
//             public static uint SwitchToFiber(IntPtr lpFiber)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     lpFiber
//                 };
//
//                 uint retValue = (uint)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"SwitchToFiber",
//                     typeof(KER32_DELEGATES.SwitchToFiber), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for IntPtr GetCurrentThread()
//             public static IntPtr GetCurrentThread()
//             {
//                 try
//                 {
//                     // Craft an array for the arguments
//                     object[] funcargs =
//                     {
//                     };
//                     IntPtr retValue = (IntPtr)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"GetCurrentThread",
//                         typeof(KER32_DELEGATES.GetCurrentThread), ref funcargs);
//                     return retValue;
//
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine($"error in GetCurrentThread: {ex.Message} ");
//                     return IntPtr.Zero;
//                 }
//                 
//             }
//             //func wrapper for bool CloseHandle(IntPtr hObject)
//             public static bool CloseHandle(IntPtr hObject)
//             {
//                 try
//                 {
//                     // Craft an array for the arguments
//                     object[] funcargs =
//                     {
//                     hObject
//                 };
//
//                     bool retValue = (bool)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"CloseHandle",
//                         typeof(KER32_DELEGATES.CloseHandle), ref funcargs);
//                     return retValue;
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine("Error in CloseHandle: " + ex.Message);
//                     Console.WriteLine(GetLastError());
//                     return false;
//                 }
//                 
//             }
//             //func wrapper for uint GetLastError();
//             public static uint GetLastError()
//             {
//                 try
//                 {
//                     // Craft an array for the arguments
//                     object[] funcargs =
//                     {
//                     };
//
//                     uint retValue = (uint)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"GetLastError",
//                         typeof(KER32_DELEGATES.GetLastError), ref funcargs);
//                     return retValue;
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine("Error in GetLastError: " + ex.Message);
//                     return 0;
//                 }  
//             }
//             //func wrapper for bool SetStdHandle(uint nStdHandle, IntPtr hHandle)
//             public static bool SetStdHandle(int nStdHandle, IntPtr hHandle)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     nStdHandle, hHandle
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"SetStdHandle",
//                     typeof(KER32_DELEGATES.SetStdHandle), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId)
//             public static IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     dwDesiredAccess, bInheritHandle, dwProcessId
//                 };
//
//                 IntPtr retValue = (IntPtr)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"OpenProcess",
//                     typeof(KER32_DELEGATES.OpenProcess), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool Wow64Process)
//             public static bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool Wow64Process)
//             {
//                 try
//                 {
//                     // Craft an array for the arguments
//                     bool temp = new bool();
//                     object[] funcargs =
//                     {
//                     hProcess, temp
//                     };
//
//                     bool retValue = (bool)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"IsWow64Process",
//                         typeof(KER32_DELEGATES.IsWow64Process), ref funcargs);
//                     Wow64Process = (bool)funcargs[1];
//                     return retValue;
//
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine("Error in IsWow64Process: " + ex.Message);
//                     Console.WriteLine(GetLastError());
//                     Wow64Process = false;
//                     return false;
//                 }
//                
//             }
//             //func wrapper for bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, ref Win32.WinBase._SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize)
//             public static bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, ref Win32.WinBase._SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize)
//             {
//                 // Craft an array for the arguments
//                 IntPtr temp1 = new IntPtr();
//                 IntPtr temp2 = new IntPtr();
//                 object[] funcargs =
//                 {
//                     temp1, temp2, lpPipeAttributes, nSize
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"CreatePipe",
//                     typeof(KER32_DELEGATES.CreatePipe), ref funcargs);
//                 hReadPipe = (IntPtr)funcargs[0];
//                 hWritePipe = (IntPtr)funcargs[1];
//                 return retValue;
//             }
//             //func wrapper for IntPtr CreateNamedPipe(string lpName, Win32.Kernel32.PipeOpenModeFlags dwOpenMode, Win32.Kernel32.PipeModeFlags dwPipeMode, uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize, uint nDefaultTimeOut, ref Win32.WinBase._SECURITY_ATTRIBUTES lpSecurityAttributes);
//             public static IntPtr CreateNamedPipe(string lpName, Win32.Kernel32.PipeOpenModeFlags dwOpenMode, Win32.Kernel32.PipeModeFlags dwPipeMode, uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize, uint nDefaultTimeOut, ref Win32.WinBase._SECURITY_ATTRIBUTES lpSecurityAttributes)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     lpName, dwOpenMode, dwPipeMode, nMaxInstances, nOutBufferSize, nInBufferSize, nDefaultTimeOut, lpSecurityAttributes
//                 };
//
//                 IntPtr retValue = (IntPtr)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"CreateNamedPipe",
//                     typeof(KER32_DELEGATES.CreateNamedPipe), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for bool PeekNamedPipe(IntPtr hNamedPipe, IntPtr lpBuffer, uint nBufferSize, out uint lpBytesRead, ref uint lpTotalBytesAvail, ref uint lpBytesLeftThisMessage)
//             public static bool PeekNamedPipe(IntPtr hNamedPipe, byte[] lpBuffer, uint nBufferSize,ref uint lpBytesRead, ref uint lpTotalBytesAvail, ref uint lpBytesLeftThisMessage)
//             {
//                 // Craft an array for the arguments
//                 uint temp1 = new uint();
//                 object[] funcargs =
//                 {
//                     hNamedPipe, lpBuffer, nBufferSize, temp1, lpTotalBytesAvail, lpBytesLeftThisMessage
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"PeekNamedPipe",
//                     typeof(KER32_DELEGATES.PeekNamedPipe), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for IntPtr CreateFile([MarshalAs(UnmanagedType.LPTStr)] string lpFileName, [MarshalAs(UnmanagedType.U4)] Win32.Kernel32.FileAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.U4)] Win32.Kernel32.FileShareFlags dwShareMode, ref Win32.WinBase._SECURITY_ATTRIBUTES lpSecurityAttributes, Win32.Kernel32.ECreationDisposition dwCreationDisposition, [MarshalAs(UnmanagedType.U4)] Win32.Kernel32.EFileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);
//             public static IntPtr CreateFile([MarshalAs(UnmanagedType.LPTStr)] string lpFileName, [MarshalAs(UnmanagedType.U4)] Win32.Kernel32.EFileAccess dwDesiredAccess, [MarshalAs(UnmanagedType.U4)] Win32.Kernel32.EFileShare dwShareMode,  IntPtr lpSecurityAttributes, Win32.Kernel32.ECreationDisposition dwCreationDisposition, [MarshalAs(UnmanagedType.U4)] Win32.Kernel32.EFileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile
//                 };
//
//                 IntPtr retValue = (IntPtr)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"CreateFile",
//                     typeof(KER32_DELEGATES.CreateFile), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for bool ReadFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped)
//             public static bool ReadFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped)
//             {
//                 // Craft an array for the arguments
//                 uint temp = new uint();
//                 object[] funcargs = { hFile, lpBuffer, nNumberOfBytesToRead, temp, lpOverlapped };
//                 try
//                 {
//                     bool retValue = (bool)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"ReadFile",typeof(KER32_DELEGATES.ReadFile), ref funcargs);
//                     lpNumberOfBytesRead = (uint)funcargs[3];
//                     return retValue;
//                 }
//                 catch (Exception ex)
//                 { 
//                     lpNumberOfBytesRead = 0;
//                     var errorCode = GetLastError();
//                     Console.WriteLine($"Error code: {errorCode}");
//                     return false;
//                 }
//                
//             }
//             //func wrapper for bool SetHandleInformation(IntPtr hObject, Win32.Kernel32.HANDLE_FLAGS dwMask, Win32.Kernel32.HANDLE_FLAGS dwFlags)
//             public static bool SetHandleInformation(IntPtr hObject, Win32.Kernel32.HANDLE_FLAGS dwMask, Win32.Kernel32.HANDLE_FLAGS dwFlags)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     hObject, dwMask, dwFlags
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"kernel32.dll", @"SetHandleInformation",
//                     typeof(KER32_DELEGATES.SetHandleInformation), ref funcargs);
//                 return retValue;
//             }
//         }
//         
//
//         public class AdvApi32FuncWrapper
//         {
//             //func wrapper for  bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, Win32.Advapi32.LOGON_TYPE dwLogonType, Win32.Advapi32.LOGON_PROVIDER dwLogonProvider, out IntPtr phToken)
//             public static bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, Win32.Advapi32.LOGON_TYPE dwLogonType, Win32.Advapi32.LOGON_PROVIDER dwLogonProvider, out IntPtr phToken)
//             {
//                 // Craft an array for the arguments
//                 IntPtr temp = new IntPtr();
//                 object[] funcargs =
//                 {
//                     lpszUsername, lpszDomain, lpszPassword, dwLogonType, dwLogonProvider, temp
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"advapi32.dll", @"LogonUser",
//                     typeof(ADVAPI32_DELEGATES.LogonUser), ref funcargs);
//                 phToken = (IntPtr)funcargs[5];
//                 return retValue;
//             }
//             //func wrapper for bool RevertToSelf();
//             public static bool RevertToSelf()
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"advapi32.dll", @"RevertToSelf",
//                     typeof(ADVAPI32_DELEGATES.RevertToSelf), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for bool ImpersonateLoggedOnUser(IntPtr hToken);
//             public static bool ImpersonateLoggedOnUser(IntPtr hToken)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     hToken
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"advapi32.dll", @"ImpersonateLoggedOnUser",
//                     typeof(ADVAPI32_DELEGATES.ImpersonateLoggedOnUser), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for bool OpenProcessToken(IntPtr ProcessHandle, uint dwDesiredAccess, out IntPtr TokenHandle);
//             public static bool OpenProcessToken(IntPtr ProcessHandle, uint dwDesiredAccess, out IntPtr TokenHandle)
//             {
//                 try
//                 {
//                     // Craft an array for the arguments
//                     IntPtr temp = new IntPtr();
//                     object[] funcargs =
//                     {
//                     ProcessHandle, dwDesiredAccess, temp
//                     };
//
//                     bool retValue = (bool)DynInv.DynamicAPIInvoke(@"advapi32.dll", @"OpenProcessToken",
//                         typeof(ADVAPI32_DELEGATES.OpenProcessToken), ref funcargs);
//                     TokenHandle = (IntPtr)funcargs[2];
//                     return retValue;
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine($"error in OpenProcessToken: {ex.Message}");
//                     Console.WriteLine(Ker32FuncWrapper.GetLastError());
//                     TokenHandle = IntPtr.Zero;
//                     return false;
//                 }
//                 
//             }
//             //func wrapper for bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, ref Win32.WinBase._SECURITY_ATTRIBUTES lpTokenAttributes, Win32.WinNT._SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, Win32.WinNT.TOKEN_TYPE TokenType, out IntPtr phNewToken);
//             public static bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, ref Win32.WinBase._SECURITY_ATTRIBUTES lpTokenAttributes, Win32.WinNT._SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, Win32.WinNT.TOKEN_TYPE TokenType, out IntPtr phNewToken)
//             {
//                 // Craft an array for the arguments
//                 IntPtr temp = new IntPtr();
//                 object[] funcargs =
//                 {
//                     hExistingToken, dwDesiredAccess, lpTokenAttributes, ImpersonationLevel, TokenType, temp
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"advapi32.dll", @"DuplicateTokenEx",
//                     typeof(ADVAPI32_DELEGATES.DuplicateTokenEx), ref funcargs);
//                 phNewToken = (IntPtr)funcargs[5];
//                 return retValue;
//             }
//             //func wrapper for bool CreateProcessWithLogonW(string lpUsername, string lpDomain, string lpPassword, uint dwLogonFlags, string lpApplicationName, string lpCommandLine, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref Win32.ProcessThreadsAPI._STARTUPINFO lpStartupInfo, out Win32.ProcessThreadsAPI._PROCESS_INFORMATION lpProcessInformation);
//             public static bool CreateProcessWithLogonW(string lpUsername, string lpDomain, string lpPassword, uint dwLogonFlags, string lpApplicationName, string lpCommandLine, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref Win32.ProcessThreadsAPI._STARTUPINFO lpStartupInfo, out Win32.ProcessThreadsAPI._PROCESS_INFORMATION lpProcessInformation)
//             {
//                 // Craft an array for the arguments
//                 Win32.ProcessThreadsAPI._PROCESS_INFORMATION temp = new Win32.ProcessThreadsAPI._PROCESS_INFORMATION();
//                 object[] funcargs =
//                 {
//                     lpUsername, lpDomain, lpPassword, dwLogonFlags, lpApplicationName, lpCommandLine, dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, temp
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"advapi32.dll", @"CreateProcessWithLogonW",
//                     typeof(ADVAPI32_DELEGATES.CreateProcessWithLogonW), ref funcargs);
//                 lpProcessInformation = (Win32.ProcessThreadsAPI._PROCESS_INFORMATION)funcargs[10];
//                 return retValue;
//             }
//             //func wrapper for bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, ref Win32.WinBase._SECURITY_ATTRIBUTES lpProcessAttributes, ref Win32.WinBase._SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref Win32.ProcessThreadsAPI._STARTUPINFO lpStartupInfo, out Win32.ProcessThreadsAPI._PROCESS_INFORMATION lpProcessInformation);
//             public static bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, ref Win32.WinBase._SECURITY_ATTRIBUTES lpProcessAttributes, ref Win32.WinBase._SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref Win32.ProcessThreadsAPI._STARTUPINFO lpStartupInfo, out Win32.ProcessThreadsAPI._PROCESS_INFORMATION lpProcessInformation)
//             {
//                 // Craft an array for the arguments
//                 Win32.ProcessThreadsAPI._PROCESS_INFORMATION temp = new Win32.ProcessThreadsAPI._PROCESS_INFORMATION();
//                 object[] funcargs =
//                 {
//                     hToken, lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, temp
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"advapi32.dll", @"CreateProcessAsUserW",
//                     typeof(ADVAPI32_DELEGATES.CreateProcessAsUser), ref funcargs);
//                 lpProcessInformation = (Win32.ProcessThreadsAPI._PROCESS_INFORMATION)funcargs[10];
//                 return retValue;
//             }
//             //func wrapper for bool GetTokenInformation(IntPtr TokenHandle, Win32.WinNT._TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength);
//             public static bool GetTokenInformation(IntPtr TokenHandle, Win32.WinNT._TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, int TokenInformationLength, out int ReturnLength)
//             {
//                 // Craft an array for the arguments
//                 int temp = new int();
//                 object[] funcargs =
//                 {
//                     TokenHandle, TokenInformationClass, TokenInformation, TokenInformationLength, temp
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"advapi32.dll", @"GetTokenInformation",
//                     typeof(ADVAPI32_DELEGATES.GetTokenInformation), ref funcargs);
//                 ReturnLength = (int)funcargs[4];
//                 return retValue;
//             }
//             //func wrapper for  bool LookupPrivilegeName(string lpSystemName, ref Win32.WinNT._LUID lpLuid, StringBuilder lpName, ref uint cchName);
//             public static bool LookupPrivilegeName(string lpSystemName, ref Win32.WinNT._LUID lpLuid, StringBuilder lpName, ref int cchName)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     lpSystemName, lpLuid, lpName, cchName
//                 }; 
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"advapi32.dll", @"LookupPrivilegeNameW",typeof(ADVAPI32_DELEGATES.LookupPrivilegeName), ref funcargs);
//                 cchName = (int)funcargs[3];
//                 return retValue;
//             }
//             //func wrapper for IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, Win32.Advapi32.SCM_ACCESS dwDesiredAccess);
//             public static IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, Win32.Advapi32.SCM_ACCESS dwDesiredAccess)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     lpMachineName, lpDatabaseName, dwDesiredAccess
//                 };
//
//                 IntPtr retValue = (IntPtr)DynInv.DynamicAPIInvoke(@"advapi32.dll", @"OpenSCManagerW",
//                     typeof(ADVAPI32_DELEGATES.OpenSCManager), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for bool StartService(IntPtr hService, uint dwNumServiceArgs, string[] lpServiceArgVectors);
//             public static bool StartService(IntPtr hService, uint dwNumServiceArgs, string[] lpServiceArgVectors)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     hService, dwNumServiceArgs, lpServiceArgVectors
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"advapi32.dll", @"StartServiceW",
//                     typeof(ADVAPI32_DELEGATES.StartService), ref funcargs);
//                 return retValue;
//             }
//             //func wrapper for  bool CloseServiceHandle(IntPtr hSCObject);
//             public static bool CloseServiceHandle(IntPtr hSCObject)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     hSCObject
//                 };
//
//                 bool retValue = (bool)DynInv.DynamicAPIInvoke(@"advapi32.dll", @"CloseServiceHandle",
//                     typeof(ADVAPI32_DELEGATES.CloseServiceHandle), ref funcargs);
//                 return retValue;
//             }
//             // func wrapper for CreateService(IntPtr hSCManager, string lpServiceName, string lpDisplayName, Win32.Advapi32.SERVICE_ACCESS dwDesiredAccess, Win32.Advapi32.SERVICE_TYPE dwServiceType, Win32.Advapi32.SERVICE_START dwStartType, Win32.Advapi32.SERVICE_ERROR dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword);
//             public static IntPtr CreateService(IntPtr hSCManager, string lpServiceName, string lpDisplayName, Win32.Advapi32.SERVICE_ACCESS dwDesiredAccess, Win32.Advapi32.SERVICE_TYPE dwServiceType, Win32.Advapi32.SERVICE_START dwStartType, Win32.Advapi32.SERVICE_ERROR dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword)
//             {
//                 // Craft an array for the arguments
//                 object[] funcargs =
//                 {
//                     hSCManager, lpServiceName, lpDisplayName, dwDesiredAccess, dwServiceType, dwStartType, dwErrorControl, lpBinaryPathName, lpLoadOrderGroup, lpdwTagId, lpDependencies, lpServiceStartName, lpPassword
//                 };
//
//                 IntPtr retValue = (IntPtr)DynInv.DynamicAPIInvoke(@"advapi32.dll", @"CreateServiceW",
//                     typeof(ADVAPI32_DELEGATES.CreateService), ref funcargs);
//                 return retValue;
//             }
//         }
//
//     }
// }
