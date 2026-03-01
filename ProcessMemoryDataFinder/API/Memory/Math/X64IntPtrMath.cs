using System;

namespace ProcessMemoryDataFinder.API.Memory.Math
{
    /// <summary>
    ///  Both target and executing processes are x64
    /// </summary>
    internal class X64IntPtrMath : IIntPtrMath
    {
        public IntPtr SumIntPtrs(IntPtr first, IntPtr second) =>
            new IntPtr(first.ToInt64() + second.ToInt64());
        public IntPtr SubstractIntPtrs(IntPtr first, IntPtr second) =>
            new IntPtr(first.ToInt64() - second.ToInt64());
    }
}
