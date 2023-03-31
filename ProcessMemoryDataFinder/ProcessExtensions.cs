using System;
using System.Diagnostics;

namespace ProcessMemoryDataFinder
{
    internal static class ProcessExtensions
    {
        internal static SafeProcess ToSafeProcess(this Process process)
        {
            if (process == null)
                return null;

            IntPtr handle;
            int pid;
            try
            {
                handle = process?.Handle ?? IntPtr.Zero;
                pid = process?.Id ?? -1;
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return new SafeProcess(process, handle, pid);
        }
    }
}