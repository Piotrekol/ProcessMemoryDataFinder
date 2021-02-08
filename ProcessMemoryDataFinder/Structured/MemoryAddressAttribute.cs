using System;

namespace ProcessMemoryDataFinder.Structured
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class MemoryAddressAttribute : Attribute
    {
        public string RelativePath { get; }

        public MemoryAddressAttribute(string relativePath)
        {
            RelativePath = relativePath;
        }
    }
}