using System;

namespace ProcessMemoryDataFinder.API.Memory.Math
{
    /// <summary>
    /// Both target and executing processes are x86
    /// </summary>
    internal class X86IntPtrMath : IIntPtrMath
    {
        public IntPtr SumIntPtrs(IntPtr first, IntPtr second)
            => IntPtr.Add(first, second.ToInt32());
        public IntPtr SubstractIntPtrs(IntPtr first, IntPtr second)
            => IntPtr.Subtract(first, second.ToInt32());
    }
}
