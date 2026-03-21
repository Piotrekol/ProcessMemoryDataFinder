using System;

namespace ProcessMemoryDataFinder.API.Memory.Math
{
    public interface IIntPtrMath
    {
        IntPtr SumIntPtrs(IntPtr first, IntPtr second);
        IntPtr SubstractIntPtrs(IntPtr first, IntPtr second);
    }
}
