using System;
using System.Collections.Generic;
using ProcessMemoryDataFinder.API.Memory.Math;

namespace ProcessMemoryDataFinder.API.Memory
{
    internal abstract class MemoryReader
    {
        public IIntPtrMath IntPtrMath { get; private set; }
        private int _intPtrSize;
        public int IntPtrSize
        {
            get => _intPtrSize;
            set
            {
                _intPtrSize = value;
                if (value == 4)
                {
                    if (Environment.Is64BitProcess)
                        IntPtrMath = new X86ProcessX64RuntimeIntPtrMath();
                    else
                        IntPtrMath = new X86IntPtrMath();
                }
                else
                    IntPtrMath = new X64IntPtrMath();
            }
        }

        public MemoryReader(int intPtrSize)
        {
            IntPtrSize = intPtrSize;
        }

        /// <summary>
        /// Reads process memory on specific operating system
        /// </summary>
        /// <param name="processHandle"></param>
        /// <param name="processPID"></param>
        /// <param name="address"></param>
        /// <param name="size"></param>
        /// <param name="targetArray"></param>
        /// <param name="bytesRead"></param>
        /// <returns>true if read call was successful</returns>
        public abstract bool ReadProcessMemory(IntPtr processHandle, int processPID, IntPtr address, uint size, byte[] targetArray, out int bytesRead);
        public abstract List<MEMORY_BASIC_INFORMATION> ReadProcessMaps(IntPtr processHandle, int processPID);
    }
}
