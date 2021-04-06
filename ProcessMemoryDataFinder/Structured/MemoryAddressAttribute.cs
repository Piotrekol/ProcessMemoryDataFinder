using System;

namespace ProcessMemoryDataFinder.Structured
{
    /// <summary>
    /// Marks property to be filled by <see cref="IStructuredMemoryReader"/> 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class MemoryAddressAttribute : Attribute
    {
        public string RelativePath { get; }
        public bool IgnoreNullPtr { get; }
        /// <summary>
        /// Marks property to be filled by <see cref="IStructuredMemoryReader"/> 
        /// </summary>
        /// <param name="relativePath">
        /// Relative or absolute memory path in the target process to property value <para/>
        /// Relative path is valid only for props and uses current class as base address for navigation. For example: "[+0xC]" would result in (pseudo-code, not valid)"[CurrentClassAddress+0xC]" <para/>
        /// Absolute path uses either predefined constant AoB pattern to search for OR static cached address defined in baseAdresses Dictionary supplied in <see cref="StructuredMemoryReader"/>
        /// </param>
        /// <param name="ignoreNullPtr">
        /// Should null-pointer reads for this property be ignored?(read won't be marked as invalid) <para/>
        /// for nullable value-types and strings this will result in prop being assigned null <para/>
        /// classes however won't be null'ed as to not disable future attempts at reading this value <para/>
        /// This can be instead achieved manually by first reading class address in a separate (int?) prop and refactoring actual prop value to be dependent on address value being non-null/zero
        /// </param>
        public MemoryAddressAttribute(string relativePath = null, bool ignoreNullPtr = false)
        {
            RelativePath = relativePath;
            IgnoreNullPtr = ignoreNullPtr;
        }
    }
}