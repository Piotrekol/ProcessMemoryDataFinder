using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using ProcessMemoryDataFinder.Structured;
using ProcessMemoryDataFinder;

namespace OsuMemoryDataProvider
{
    public class StructuredOsuMemoryReader : IStructuredMemoryReader, IDisposable
    {
        private StructuredMemoryReader _memoryReader;
        private static readonly ConcurrentDictionary<ProcessTargetOptions, StructuredOsuMemoryReader> Instances = [];
        private static StructuredOsuMemoryReader instance;

        public OsuBaseAddresses OsuMemoryAddresses { get; } = new OsuBaseAddresses();
        /// <summary>
        ///     It is strongly encouraged to use single <see cref="StructuredOsuMemoryReader" /> instance in order to not have to duplicate
        ///     find-pattern-location work
        /// </summary>
        public static StructuredOsuMemoryReader Instance => instance ??= new StructuredOsuMemoryReader(new("osu!"));

        public bool WithTimes
        {
            get => _memoryReader.WithTimes;
            set => _memoryReader.WithTimes = value;
        }
        public Dictionary<string, double> ReadTimes => _memoryReader.ReadTimes;
        public bool CanRead => _memoryReader.CanRead;
        public bool AbortReadOnInvalidValue
        {
            get => _memoryReader.AbortReadOnInvalidValue;
            set => _memoryReader.AbortReadOnInvalidValue = value;
        }
        public event EventHandler<(object readObject, string propPath)> InvalidRead
        {
            add => _memoryReader.InvalidRead += value;
            remove => _memoryReader.InvalidRead -= value;
        }
        public int ProcessWatcherDelayMs
        {
            get => _memoryReader.ProcessWatcherDelayMs;
            set => _memoryReader.ProcessWatcherDelayMs = value;
        }

        public static StructuredOsuMemoryReader GetInstance(ProcessTargetOptions processTargetOptions)
        {
            if (processTargetOptions is null)
            {
                return Instance;
            }

            return Instances.GetOrAdd(processTargetOptions, s => new StructuredOsuMemoryReader(processTargetOptions));
        }

        public Dictionary<string, string> BaseAddresses { get; } = new Dictionary<string, string>
        {
            //class pointers
            {"Base", "F80174048365"},
            {"CurrentBeatmap","[Base-0xC]"},
            {"CurrentSkinData","[85C074118B1D+0x6]"},
            {"CurrentRuleset","[C7864801000001000000A1+0xB]+0x4"},// or backup: 7D15A1????????85C0-B]+4 //TourneyBase
            {"Settings", "[83E02085C07E2F+0x8]"},
            {"TotalAudioTimeBase", "[83E4F8575683EC38+0xA]" },
            {"UserPanel", "[FFFF895070+0x6]" },

            //static values
            {"OsuStatus", "[4883F804731E-0x4]"},//[Base-0x3C]
            {"GameMode", "[Base-0x33]"},
            {"Retries", "[Base-0x33]+0x8"},
            {"AudioTime","[Base+0x64]-0x10"},
            {"Mods","[C8FF??????????810D????????00080000+0x9]"},
            {"IsReplay","[8BFAB801000000+0x2A]"},
            {"ChatIsExpanded","0AD7233C0000??01-0x20"},
            {"IsLoggedIn", "[B80B00008B35-0xB]" }
        };

        public StructuredOsuMemoryReader(ProcessTargetOptions processTargetOptions)
        {
            _memoryReader = new MultiplayerPlayerStructuredMemoryReader("osu!", BaseAddresses, processTargetOptions);
        }
        public bool TryRead<T>(T readObj) where T : class
            => _memoryReader.TryRead(readObj);

        public bool TryReadProperty(object readObj, string propertyName, out object result)
            => _memoryReader.TryReadProperty(readObj, propertyName, out result);
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

        protected class MultiplayerPlayerStructuredMemoryReader : StructuredMemoryReader
        {
            public MultiplayerPlayerStructuredMemoryReader(string processName, Dictionary<string, string> baseAdresses, ProcessTargetOptions processTargetOptions) : base(processName, baseAdresses, processTargetOptions)
            {
                ObjectReader.IntPtrSize = _addressFinder.IntPtrSize = _memoryReader.IntPtrSize = 4;
                AddReadHandlers(new Dictionary<Type, ReadObject>
                {
                    { typeof(List<MultiplayerPlayer>), ReadList },
                    { typeof(List<PlayerScore>), ReadList }
                });
            }

            private IList ReadList(IntPtr finalAddress, PropInfo propInfo)
            {
                if (finalAddress == IntPtr.Zero)
                    return null;

                var classPointers = ObjectReader.ReadUIntList(finalAddress);
                var propListValue = (IList)propInfo.Getter();
                if (classPointers == null || classPointers.Count == 0 || classPointers.Count > propListValue.Count)
                    return propListValue;

                var rootPath = $"{propInfo.Path}*{classPointers.Count}";
                for (int i = 0; i < classPointers.Count; i++)
                {
                    if (classPointers[i] > IntPtrExtensions.MaxValue)
                        return propListValue;

                    TryInternalRead(propListValue[i], new IntPtr(classPointers[i]), $"{rootPath}[{i}]");
                }

                return propListValue;
            }
        }
    }
}