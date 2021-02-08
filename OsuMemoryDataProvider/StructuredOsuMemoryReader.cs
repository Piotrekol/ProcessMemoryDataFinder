﻿using System;
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

        public bool WithTimes
        {
            get => _memoryReader.WithTimes;
            set => _memoryReader.WithTimes = value;
        } 
        public Dictionary<string, double> ReadTimes => _memoryReader.ReadTimes;


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
            {"OsuStatus", "[Base-0x3C]"},
            {"GameMode", "[Base-0x33]"},
            {"Retries", "[Base-0x33]+0x8"},
            {"AudioTime","[Base+0x64]-0x10"},
            {"Mods","[C8FF??????????810D????????00080000+0x9]"},
            {"IsReplay","[741A80????????????741180+0xD]"},
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