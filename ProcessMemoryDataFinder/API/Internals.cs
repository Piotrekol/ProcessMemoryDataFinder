using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ProcessMemoryDataFinder.API
{
    internal class MemoryProcessAddressFinder
    {
        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

        public List<MEMORY_BASIC_INFORMATION> MemReg { get; set; } = new List<MEMORY_BASIC_INFORMATION>();
        
        /// <summary>
        /// Finds process fragmented memory information and saves it to <see cref="MemReg"/> property
        /// </summary>
        /// <param name="pHandle">The p handle.</param>
        public void MemInfo(IntPtr pHandle)
        {
            IntPtr addy = new IntPtr();
            while (true)
            {
                MEMORY_BASIC_INFORMATION memInfo = new MEMORY_BASIC_INFORMATION();
                int memDump = VirtualQueryEx(pHandle, addy, out memInfo, Marshal.SizeOf(memInfo));
                if (memDump == 0) break;
                if ((memInfo.State & 0x1000) != 0 && (memInfo.Protect & 0x100) == 0)
                    MemReg.Add(memInfo);
#if x64
                addy = new IntPtr(memInfo.BaseAddress.ToInt64() + memInfo.RegionSize.ToInt64());
#else
                addy = new IntPtr(memInfo.BaseAddress.ToInt32() + memInfo.RegionSize.ToInt32());
#endif
            }
        }
    }
}