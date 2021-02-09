using System;

namespace ProcessMemoryDataFinder.Structured
{
    /// <summary>
    /// Sets class/prop relative memory path
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class MemoryAddressAttribute : Attribute
    {
        public string RelativePath { get; }

        public MemoryAddressAttribute(string relativePath = null)
        {
            RelativePath = relativePath;
        }
    }
}