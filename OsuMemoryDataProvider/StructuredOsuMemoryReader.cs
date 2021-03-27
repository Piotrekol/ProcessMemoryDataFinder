using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider
{
    public class StructuredOsuMemoryReader : IStructuredMemoryReader, IDisposable
    {
        private StructuredMemoryReader _memoryReader;
        public OsuBaseAddresses OsuMemoryAddresses { get; } = new OsuBaseAddresses();
        /// <summary>
        ///     It is strongly encouraged to use single <see cref="StructuredOsuMemoryReader" /> instance in order to not have to duplicate
        ///     find-pattern-location work
        /// </summary>
        public static StructuredOsuMemoryReader Instance { get; } = new StructuredOsuMemoryReader();
        private static readonly ConcurrentDictionary<string, StructuredOsuMemoryReader> Instances =
            new ConcurrentDictionary<string, StructuredOsuMemoryReader>();

        public bool WithTimes
        {
            get => _memoryReader.WithTimes;
            set => _memoryReader.WithTimes = value;
        }
        public Dictionary<string, double> ReadTimes => _memoryReader.ReadTimes;
        public bool AbortReadOnInvalidValue
        {
            get => _memoryReader.AbortReadOnInvalidValue;
            set => _memoryReader.AbortReadOnInvalidValue = value;
        }


        public StructuredOsuMemoryReader GetInstanceForWindowTitleHint(string windowTitleHint)
        {
            if (string.IsNullOrEmpty(windowTitleHint)) return Instance;
            return Instances.GetOrAdd(windowTitleHint, s => new StructuredOsuMemoryReader(s));
        }

        public Dictionary<string, string> BaseAddresses { get; } = new Dictionary<string, string>
        {
            //class pointers
            {"Base", "F80174048365"},
            {"CurrentBeatmap","[Base-0xC]"},
            {"CurrentSkinData","[75218B1D+0x4]"},
            {"CurrentRuleset","[C7864801000001000000A1+0xB]+0x4"},// or backup: 7D15A1????????85C0-B]+4 //TourneyBase

            //static values
            {"OsuStatus", "[4883F804731E-0x4]"},//[Base-0x3C]
            {"GameMode", "[Base-0x33]"},
            {"Retries", "[Base-0x33]+0x8"},
            {"AudioTime","[Base+0x64]-0x10"},
            {"Mods","[C8FF??????????810D????????00080000+0x9]"},
            {"IsReplay","[741A80????????????741180+0xD]"},
        };

        public StructuredOsuMemoryReader(string mainWindowTitleHint = null)
        {
            _memoryReader = new MultiplayerPlayerStructuredMemoryReader("osu!", BaseAddresses, mainWindowTitleHint);
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
            public MultiplayerPlayerStructuredMemoryReader(string processName, Dictionary<string, string> baseAdresses, string mainWindowTitleHint = null) : base(processName, baseAdresses, mainWindowTitleHint)
            {
                ObjectReader.IntPtrSize = 4;
                _addressFinder.IntPtrSize = 4;
            }

            protected override object ReadObjectAt(IntPtr finalAddress, PropInfo propInfo)
            {
                if (finalAddress == IntPtr.Zero) return null;
                if (propInfo.PropType == typeof(List<MultiplayerPlayer>))
                {
                    var playerClassPointers = ObjectReader.ReadIntList(finalAddress);
                    var playerList = (List<MultiplayerPlayer>)propInfo.Getter();
                    if (playerClassPointers.Count == 0 || playerClassPointers.Count > playerList.Count)
                        return playerList;

                    var path = $"{propInfo.Path}[*{playerClassPointers.Count}]";
                    for (int i = 0; i < playerClassPointers.Count; i++)
                    {
                        TryInternalRead(playerList[i], new IntPtr(playerClassPointers[i]), path);
                    }

                    return playerList;
                }

                return base.ReadObjectAt(finalAddress, propInfo);
            }
        }
    }
}