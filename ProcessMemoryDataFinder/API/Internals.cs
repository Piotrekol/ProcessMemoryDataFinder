using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ProcessMemoryDataFinder.API
{
    internal class X64MemoryProcessAddressFinder : MemoryProcessAddressFinder
    {
        protected override IntPtr SumIntPtrs(IntPtr first, IntPtr second) =>
            new IntPtr(first.ToInt64() + second.ToInt64());
    }
    internal class X86ProcessX86RuntimeAddressFinder : MemoryProcessAddressFinder
    {
        protected override IntPtr SumIntPtrs(IntPtr first, IntPtr second)
            => IntPtr.Add(first, second.ToInt32());
    }

    internal class X86ProcessX64RuntimeAddressFinder : MemoryProcessAddressFinder
    {
        protected override IntPtr SumIntPtrs(IntPtr first, IntPtr second)
            => new IntPtr(first.ToInt64() + second.ToInt64());
    }

    internal abstract class MemoryProcessAddressFinder
    {
        public int IntPtrSize { get; set; } = IntPtr.Size;

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

        /// <summary>
        /// Finds process fragmented memory information
        /// </summary>
        /// <param name="pHandle">process handle</param>
        public List<MEMORY_BASIC_INFORMATION> MemInfo(IntPtr pHandle)
        {
            IntPtr addy = new IntPtr();
            var result = new List<MEMORY_BASIC_INFORMATION>();

            while (true)
            {
                MEMORY_BASIC_INFORMATION memInfo = new MEMORY_BASIC_INFORMATION();
                int memDump = VirtualQueryEx(pHandle, addy, out memInfo, Marshal.SizeOf(memInfo));
                if (memDump == 0) break;
                if ((memInfo.State & 0x1000) != 0 && (memInfo.Protect & 0x100) == 0)
                    result.Add(memInfo);

                addy = SumIntPtrs(memInfo.BaseAddress, memInfo.RegionSize);
            }

            return result;
        }

        protected abstract IntPtr SumIntPtrs(IntPtr first, IntPtr second);
    }
}