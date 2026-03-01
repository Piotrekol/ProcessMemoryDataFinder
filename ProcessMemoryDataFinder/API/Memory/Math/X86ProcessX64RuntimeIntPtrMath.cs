using System;

namespace ProcessMemoryDataFinder.API.Memory.Math
{
    /// <summary>
    /// Target process is x86 while executing process is x64
    /// </summary>
    internal class X86ProcessX64RuntimeIntPtrMath : IIntPtrMath
    {
        public IntPtr SumIntPtrs(IntPtr first, IntPtr second)
            => new IntPtr(first.ToInt64() + second.ToInt64());
        public IntPtr SubstractIntPtrs(IntPtr first, IntPtr second)
            => new IntPtr(first.ToInt64() - second.ToInt64());
    }
}
