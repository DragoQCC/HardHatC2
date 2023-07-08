using System;
using System.Runtime.InteropServices;

namespace DynamicEngLoading
{
    public unsafe class h_coff
    {
        public static string beaconOutputData;
        public static int beaconOutputData_sz = 0;

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct COFF_FILE_HEADER
        {
            public ushort Machine;
            public ushort NumberOfSections;
            public int TimeDateStamp;
            public int PointerToSymbolTable;
            public int NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct COFF_SECT
        {
            public fixed byte Name[8];
            public int VirtualSize;
            public int VirtualAddress;
            public int SizeOfRawData;
            public int PointerToRawData;
            public int PointerToRelocations;
            public int PointerToLineNumbers;
            public ushort NumberOfRelocations;
            public ushort NumberOfLinenumbers;
            public int Characteristics;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct COFF_RELOC
        {
            public int VirtualAddress;
            public int SymbolTableIndex;
            public ushort Type;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public unsafe struct COFF_SYM
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public fixed byte Name[8];

            [System.Runtime.InteropServices.FieldOffset(0)]
            public fixed int value_u[2];

            [System.Runtime.InteropServices.FieldOffset(8)]
            public int Value;

            [System.Runtime.InteropServices.FieldOffset(0xc)]
            public ushort SectionNumber;

            [System.Runtime.InteropServices.FieldOffset(0xe)]
            public ushort Type;

            [System.Runtime.InteropServices.FieldOffset(0x10)]
            public byte StorageClass;

            [System.Runtime.InteropServices.FieldOffset(0x11)]
            public byte NumberOfAuxSymbols;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct BEACON_FUNCTION
        {
            public uint hash;
            public void* function;

            public BEACON_FUNCTION(uint hash, void* function)
            {
                this.hash = hash;
                this.function = function;
            }
        }

        public unsafe class StructHelper
        {
            static public unsafe string ConvertToString(byte* arr)
            {
                return new string((sbyte*)arr);
            }

            static public string PrintStruct(COFF_FILE_HEADER* data)
            {
                string ret = "";
                ret += "COFF_FILE_HEADER\n";
                ret += String.Format("\tMachine:              0x{0:X}\n", data->Machine);
                ret += String.Format("\tNumberOfSections:     0x{0:X}\n", data->NumberOfSections);
                ret += String.Format("\tTimeDateStamp:        0x{0:X}\n", data->TimeDateStamp);
                ret += String.Format("\tPointerToSymbolTable: 0x{0:X}\n", data->PointerToSymbolTable);
                ret += String.Format("\tNumberOfSymbols:      0x{0:X}\n", data->NumberOfSymbols);
                ret += String.Format("\tSizeOfOptionalHeader: 0x{0:X}\n", data->SizeOfOptionalHeader);
                ret += String.Format("\tCharacteristics:      0x{0:X}\n", data->Characteristics);
                return ret;
            }

            static public string PrintStruct(COFF_SECT* data)
            {
                string ret = "";
                ret += "COFF_SECT\n";
                ret += String.Format("\tName:                 {0}\n", ConvertToString(data->Name));
                ret += String.Format("\tVirtualSize:          0x{0:X}\n", data->VirtualSize);
                ret += String.Format("\tVirtualAddress:       0x{0:X}\n", data->VirtualAddress);
                ret += String.Format("\tSizeOfRawData:        0x{0:X}\n", data->SizeOfRawData);
                ret += String.Format("\tPointerToRawData:     0x{0:X}\n", data->PointerToRawData);
                ret += String.Format("\tPointerToRelocations: 0x{0:X}\n", data->PointerToRelocations);
                ret += String.Format("\tPointerToLineNumbers: 0x{0:X}\n", data->PointerToLineNumbers);
                ret += String.Format("\tNumberOfRelocations:  0x{0:X}\n", data->NumberOfRelocations);
                ret += String.Format("\tNumberOfLineNumbers:  0x{0:X}\n", data->NumberOfLinenumbers);
                ret += String.Format("\tCharacteristics:      0x{0:X}\n", data->Characteristics);
                return ret;
            }

            static public string PrintStruct(COFF_RELOC* data)
            {
                string ret = "";
                ret += "COFF_RELOC\n";
                ret += String.Format("\tVirtualAddress:       0x{0:X}\n", data->VirtualAddress);
                ret += String.Format("\tSymbolTableIndex:     0x{0:X}\n", data->SymbolTableIndex);
                ret += String.Format("\tType:                 0x{0:X}\n", data->Type);
                return ret;
            }

            static public string PrintStruct(COFF_SYM* data)
            {
                string ret = "";
                ret += "COFF_SYM\n";
                ret += String.Format("\tName:                 {0}\n", ConvertToString(data->Name));
                ret += String.Format("\tvalue_u[0]:           0x{0:X}\n", data->value_u[0]);
                ret += String.Format("\tvalue_u[1]:           0x{0:X}\n", data->value_u[1]);
                ret += String.Format("\tValue:                0x{0:X}\n", data->Value);
                ret += String.Format("\tSectionNumber:        0x{0:X}\n", data->SectionNumber);
                ret += String.Format("\tType:                 0x{0:X}\n", data->Type);
                ret += String.Format("\tStorageClass:         0x{0:X}\n", data->StorageClass);
                ret += String.Format("\tNumberOfAuxSymbols:   0x{0:X}\n", data->NumberOfAuxSymbols);
                return ret;
            }
        }

        public unsafe class Win32
        {
            [Flags]
            public enum AllocationType
            {
                NULL = 0x0,
                Commit = 0x1000,
                Reserve = 0x2000,
                Decommit = 0x4000,
                Release = 0x8000,
                Reset = 0x80000,
                Physical = 0x400000,
                TopDown = 0x100000,
                WriteWatch = 0x200000,
                LargePages = 0x20000000
            }

            public enum MemoryProtection : UInt32
            {
                PAGE_EXECUTE = 0x00000010,
                PAGE_EXECUTE_READ = 0x00000020,
                PAGE_EXECUTE_READWRITE = 0x00000040,
                PAGE_EXECUTE_WRITECOPY = 0x00000080,
                PAGE_NOACCESS = 0x00000001,
                PAGE_READONLY = 0x00000002,
                PAGE_READWRITE = 0x00000004,
                PAGE_WRITECOPY = 0x00000008,
                PAGE_GUARD = 0x00000100,
                PAGE_NOCACHE = 0x00000200,
                PAGE_WRITECOMBINE = 0x00000400
            }

            public static int IMAGE_REL_AMD64_ADDR64 = 0x0001; // TODO: Move this to a global area
            public static int IMAGE_REL_AMD64_ADDR32NB = 0x0003;

            /* Most common from the looks of it, just 32-bit relative address from the byte following the relocation */
            public static int IMAGE_REL_AMD64_REL32 = 0x0004;
            public static int IMAGE_REL_AMD64_REL32_5 = 0x0009;

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern uint GetLastError();

            [DllImport(
                "msvcrt.dll",
                EntryPoint = "memcpy",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = false
            )]
            public static extern IntPtr memcpy(IntPtr dest, byte[] src, UInt32 count);

            [DllImport(
                "msvcrt.dll",
                EntryPoint = "memcpy",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = false
            )]
            public static extern void* Memcpy(byte* dest, byte* src, void* count);

            [DllImport("kernel32.dll")]
            public static extern IntPtr VirtualAlloc(
                IntPtr lpAddress,
                uint dwSize,
                uint flAllocationType,
                uint flProtect
            );

            [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
            public static extern bool VirtualFreeEx(
                IntPtr hProcess,
                IntPtr lpAddress,
                IntPtr dwSize,
                AllocationType dwFreeType
            );

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);
        }
    }
}
