using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessMemoryDataFinder
{
    public static class IntPtrExtensions
    {
        public static long MaxValue = 0;

        private static uint x86IntPtrMax = 0x7fffffff;
        private static long x64IntPtrMax = 0x7fffffffffffffff;
        static IntPtrExtensions()
        {
#if NET5_0_OR_GREATER
            MaxValue = IntPtr.MaxValue.ToInt64();
#else
            MaxValue = Environment.Is64BitProcess
                ? x64IntPtrMax
                : x86IntPtrMax;
#endif
        }
    }
}
