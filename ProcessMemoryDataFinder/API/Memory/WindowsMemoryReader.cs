using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ProcessMemoryDataFinder.API.Memory
{
    internal class WindowsMemoryReader : MemoryReader
    {
        public WindowsMemoryReader(int intPtrSize) : base(intPtrSize)
        {
        }

        public override bool ReadProcessMemory(IntPtr processHandle, int processPID, IntPtr address, uint size, byte[] targetArray, out int bytesRead)
        {
            try
            {
                ReadProcessMemory(processHandle, address, targetArray, (int)size, out bytesRead);
                return size == bytesRead;
            }
            catch
            {
                bytesRead = 0;
                return false;
            }
        }

        public override List<MEMORY_BASIC_INFORMATION> ReadProcessMaps(IntPtr processHandle, int processPID)
        {
            var result = new List<MEMORY_BASIC_INFORMATION>();
            var lastRegionEndAddress = new IntPtr();
            while (true)
            {
                MEMORY_BASIC_INFORMATION memInfo = new MEMORY_BASIC_INFORMATION();
                int memDump = VirtualQueryEx(processHandle, lastRegionEndAddress, out memInfo, Marshal.SizeOf(memInfo));
                if (memDump == 0) break;
                if ((memInfo.State & 0x1000) != 0 && (memInfo.Protect & 0x100) == 0)
                    result.Add(memInfo);

                lastRegionEndAddress = IntPtrMath.SumIntPtrs(memInfo.BaseAddress, memInfo.RegionSize);
            }

            return result;
        }

        /// <summary>
        /// ReadProcessMemory
        /// 
        ///     API import definition for ReadProcessMemory.
        /// </summary>
        /// <param name="hProcess">Handle to the process we want to read from.</param>
        /// <param name="lpBaseAddress">The base address to start reading from.</param>
        /// <param name="lpBuffer">The return buffer to write the read data to.</param>
        /// <param name="dwSize">The size of data we wish to read.</param>
        /// <param name="lpNumberOfBytesRead">The number of bytes successfully read.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true)]
        protected static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead
            );

        [DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx", SetLastError = true)]
        protected static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);
    }
}
