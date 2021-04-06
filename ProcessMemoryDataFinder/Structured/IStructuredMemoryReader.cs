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
        /// Should an attempt at reading invalid address in <see cref="TryRead{T}"/> abort read call(and return false)?
        /// </summary>
        bool AbortReadOnInvalidValue { get; set; }

        event EventHandler<(object readObject, string propPath)> InvalidRead;

        /// <summary>
        /// Recursively reads all props in <see cref="T"/> hierarchy marked with <see cref="MemoryAddressAttribute"/>
        /// </summary>
        /// <typeparam name="T">class to read</typeparam>
        /// <param name="readObj"><see cref="T"/></param>
        /// <param name="propertyNameToRead"></param>
        /// <returns></returns>
        bool TryRead<T>(T readObj) where T : class;

        /// <summary>
        /// Reads single prop value in <see cref="readObj"/>
        /// </summary>
        /// <param name="readObj"></param>
        /// <param name="propertyNameToRead"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryReadProperty(object readObj, string propertyName, out object result);
    }
}