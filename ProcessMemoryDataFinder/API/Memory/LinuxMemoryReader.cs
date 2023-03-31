using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ProcessMemoryDataFinder.API.Memory
{
    internal class LinuxMemoryReader : MemoryReader
    {
        public LinuxMemoryReader(int intPtrSize) : base(intPtrSize)
        {
        }

        public override List<MEMORY_BASIC_INFORMATION> ReadProcessMaps(IntPtr processHandle, int processPID)
        {
            var result = new List<MEMORY_BASIC_INFORMATION>();
            var rawProcessMaps = File.ReadAllLines($"/proc/{processPID}/maps");
            for (int i = 0; i < rawProcessMaps.Length; i++)
            {
                var line = rawProcessMaps[i].AsSpan();
                var addressStrLength = line.IndexOf('-');
                var startAddressSpan = line[0..addressStrLength];
                var endAddressSpan = line[(addressStrLength + 1)..(addressStrLength * 2 + 1)];
                var flagsSpan = line[(addressStrLength * 2 + 2)..(addressStrLength * 2 + 6)];

                var memInfo = new MEMORY_BASIC_INFORMATION();
                memInfo.BaseAddress = new IntPtr(long.Parse(startAddressSpan, System.Globalization.NumberStyles.HexNumber));
                var endRegionAddress = new IntPtr(long.Parse(endAddressSpan, System.Globalization.NumberStyles.HexNumber));
                memInfo.RegionSize = IntPtrMath.SubstractIntPtrs(endRegionAddress, memInfo.BaseAddress);
                //flags: rwxp
                if (flagsSpan[1] != '-')
                    result.Add(memInfo);
            }

            return result;
        }

        public override bool ReadProcessMemory(IntPtr processHandle, int processPID, IntPtr address, uint size, byte[] targetArray, out int bytesRead)
            => ReadProcessMemoryUnsafe(address, processPID, size, targetArray, out bytesRead);

        protected static unsafe bool ReadProcessMemoryUnsafe(IntPtr address, int processId, uint size, byte[] dumpArray, out int bytesRead)
        {
            //TODO: create block of memory upfront and recreate if size is less than requested.
            var dumpPointer = Marshal.AllocCoTaskMem(dumpArray.Length);
            try
            {
                var localIo = new iovec
                {
                    iov_base = dumpPointer.ToPointer(),
                    iov_len = (int)size
                };
                var remoteIo = new iovec
                {
                    iov_base = address.ToPointer(),
                    iov_len = (int)size
                };

                bytesRead = process_vm_readv(processId, &localIo, 1, &remoteIo, 1, 0);
                if (bytesRead < 0)
                    return false;

                //TODO: use spans
                Marshal.Copy(dumpPointer, dumpArray, 0, bytesRead);
            }
            finally
            {
                Marshal.FreeCoTaskMem(dumpPointer);
            }

            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct iovec
        {
            public void* iov_base;
            public int iov_len;
        }

        /// <summary>
        /// https://linux.die.net/man/2/process_vm_readv
        /// </summary>
        /// <param name="pid">Process Id</param>
        /// <param name="local_iov">return struct buffer(s) to write the read data to.</param>
        /// <param name="liovcnt">Amount of return structs provided. Unused here, always 1</param>
        /// <param name="remote_iov">struct(s) containing address to read data from.</param>
        /// <param name="riovcnt">Amount of address structs provided. Unused here, always 1</param>
        /// <param name="flags">Unused, must be 0</param>
        /// <returns>Amount of bytes read or -1 on error</returns>
        [DllImport("libc", SetLastError = true)]
        private static extern unsafe int process_vm_readv(int pid,
            iovec* local_iov,
            ulong liovcnt,
            iovec* remote_iov,
            ulong riovcnt,
            ulong flags);
    }
}
