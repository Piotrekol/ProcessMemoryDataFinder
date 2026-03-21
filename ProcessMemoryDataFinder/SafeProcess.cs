using System;
using System.Diagnostics;

namespace ProcessMemoryDataFinder
{
    public class SafeProcess : IDisposable
    {
        public Process Process { get; }
        public IntPtr Handle { get; }
        public int PID { get; }
        internal SafeProcess(Process process, IntPtr handle, int pID)
        {
            Process = process;
            Handle = handle;
            PID = pID;
        }

        public bool HasExited()
        {
            try
            {
                return Process.HasExited;
            }
            catch
            {
                return true;
            }
        }

        public void Dispose()
        {
            Process.Dispose();
        }
    }
}
