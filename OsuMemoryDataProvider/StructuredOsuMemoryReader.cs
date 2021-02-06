using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider
{
    public class StructuredOsuMemoryReader : IDisposable
    {
        private StructuredMemoryReader _memoryReader;

        /// <summary>
        ///     It is strongly encouraged to use single <see cref="StructuredOsuMemoryReader" /> instance in order to not have to duplicate
        ///     find-pattern-location work
        /// </summary>
        public static StructuredOsuMemoryReader Instance { get; } = new StructuredOsuMemoryReader();
        private static readonly ConcurrentDictionary<string, StructuredOsuMemoryReader> Instances =
            new ConcurrentDictionary<string, StructuredOsuMemoryReader>();

        public StructuredOsuMemoryReader GetInstanceForWindowTitleHint(string windowTitleHint)
        {
            if (string.IsNullOrEmpty(windowTitleHint)) return Instance;
            return Instances.GetOrAdd(windowTitleHint, s => new StructuredOsuMemoryReader(s));
        }

        public Dictionary<string, string> BaseAddresses { get; } = new Dictionary<string, string>
        {
            {"Base", "F80174048365"},
            {"CurrentBeatmap","[Base-0xC]"},
            {"OsuStatus", "[Base-0x3C]"},
            {"CurrentSkinData","[75218B1D+0x4]"},
            {"CurrentRuleset","[C7864801000001000000A1+0xB]+0x4"},// or backup: 7D15A1????????85C0-B]+4 //TourneyBase
            {"IsReplay","[[741A80????????????741180+0xD]]"},

            {"Mods","[[C8FF??????????810D????????00080000+0x9]]"},
        };

        public StructuredOsuMemoryReader(string mainWindowTitleHint = null)
        {
            _memoryReader = new StructuredMemoryReader("osu!", BaseAddresses, mainWindowTitleHint);
        }

        public T Read<T>(T readObj) where T : class, new()
            => _memoryReader.Read(readObj);


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _memoryReader?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}