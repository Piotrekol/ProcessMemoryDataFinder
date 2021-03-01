using System;
using System.Collections.Generic;

namespace ProcessMemoryDataFinder.Structured
{
    public interface IStructuredMemoryReader : IDisposable
    {
        /// <summary>
        /// Should memory read times be tracked and saved in <see cref="ReadTimes"/>?
        /// </summary>
        bool WithTimes { get; set; }

        /// <summary>
        /// When <see cref="WithTimes"/> is true, stores per-prop read times
        /// </summary>
        Dictionary<string, double> ReadTimes { get; }

        /// <summary>
        /// Recursively reads all props in <see cref="T"/> hierarchy marked with <see cref="MemoryAddressAttribute"/>
        /// </summary>
        /// <typeparam name="T">class to read</typeparam>
        /// <param name="readObj"><see cref="T"/></param>
        /// <param name="propertyNameToRead"></param>
        /// <returns></returns>
        T Read<T>(T readObj) where T : class;

        /// <summary>
        /// Reads single prop value in <see cref="readObj"/>
        /// </summary>
        /// <param name="readObj"></param>
        /// <param name="propertyNameToRead"></param>
        /// <returns></returns>
        object ReadProperty(object readObj, string propertyName);
    }
}