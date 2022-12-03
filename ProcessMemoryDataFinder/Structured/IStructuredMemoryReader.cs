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
        /// Is current reader instance ready for read calls (is there valid process available)<para/>
        /// Reads started while this is false will always fail
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// How often should reader check if valid process is available on the system.
        /// </summary>
        int ProcessWatcherDelayMs { get; set; }

        /// <summary>
        /// Should an attempt at reading invalid address in <see cref="TryRead{T}"/> abort read call(and return false)?
        /// </summary>
        bool AbortReadOnInvalidValue { get; set; }

        /// <summary>
        /// Triggers whenever memory read results in invalid address or null pointer
        /// </summary>
        event EventHandler<(object readObject, string propPath)> InvalidRead;

        /// <summary>
        /// Recursively reads all props in <see cref="T"/> hierarchy marked with <see cref="MemoryAddressAttribute"/>
        /// </summary>
        /// <typeparam name="T">class to read</typeparam>
        /// <param name="readObj"><see cref="T"/></param>
        /// <param name="propertyNameToRead"></param>
        /// <returns>false if only part of read completed, otherwise true</returns>
        bool TryRead<T>(T readObj) where T : class;

        /// <summary>
        /// Reads single prop value in <see cref="readObj"/>
        /// </summary>
        /// <param name="readObj">base object with contains property that is going to be read</param>
        /// <param name="propertyNameToRead">property name to be read (use nameof())</param>
        /// <param name="result">new value of the prop from memory</param>
        /// <returns>false if only part of read completed, otherwise true</returns>
        bool TryReadProperty(object readObj, string propertyName, out object result);
    }
}