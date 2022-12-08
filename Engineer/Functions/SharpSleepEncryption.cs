//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;
//using Engineer.Extra;

//namespace Engineer.Functions
//{
//    internal unsafe class SharpSleepEncryption
//    {
//        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
//        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, FreeType dwFreeType);

//        [DllImport("kernel32.dll", SetLastError = true)]
//        public static extern IntPtr GetModuleHandle(string lpModuleName);

//        //Load Library kernel32 
//        [DllImport("kernel32.dll", SetLastError = true)]
//        public static extern IntPtr LoadLibrary(string library);
//        //get proc address kernel32

//        [DllImport("kernel32.dll", SetLastError = true)]
//        public static extern IntPtr GetProcAddress(IntPtr libPtr, string function);

//        //dll import for VirtualAlloc
//        [DllImport("kernel32.dll", SetLastError = true)]
//        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, h_reprobate.Win32.Kernel32.AllocationType flAllocationType, h_reprobate.Win32.Kernel32.MemoryProtection flProtect);

//        //dll import for GetCurrentThreadId
//        [DllImport("kernel32.dll", SetLastError = true)]
//        public static extern uint GetCurrentThreadId();

//        [Flags]
//        public enum FreeType
//        {
//            Decommit = 0x4000,
//            Release = 0x8000,
//        }


//        public static h_reprobate.PE.IMAGE_DOS_HEADER* dosHeader = (h_reprobate.PE.IMAGE_DOS_HEADER*)Process.GetCurrentProcess().MainModule.BaseAddress;
//        public static h_reprobate.PE.IMAGE_OPTIONAL_HEADER64? optionalHeader = null;
//        public static h_reprobate.PE.IMAGE_FILE_HEADER? fileHeader = null;
//        public static h_reprobate.PE.IMAGE_SECTION_HEADER? sectionHeader = null;

//        //array for storing different memory protection values for each section
//        public static uint[] sectionProtection = new uint[256];


//        public static void ExecuteSleep(int sleeptime)
//        {
//            //process and thread IDs 
//            // get current process id
//            int pid = Process.GetCurrentProcess().Id;

//            //get current thread id
//            uint tid = GetCurrentThreadId();

//            //get a pointer to the nt header
//            h_reprobate.PE.IMAGE_NT_HEADERS64* ntHeader = (h_reprobate.PE.IMAGE_NT_HEADERS64*)((byte*)dosHeader + dosHeader->e_lfanew);

//        }

//    }
//}
