using System.Diagnostics;

namespace ProcessMemoryDataFinder.Misc
{
    internal static class ProcessExtensions
    {
        internal static bool SafeHasExited(this Process process)
        {

            try
            {
                return process.HasExited;
            }
            catch
            {
                return true;
            }
        }
    }
}