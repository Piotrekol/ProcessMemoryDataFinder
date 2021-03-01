﻿using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.OsuMemoryModels.Direct
{
    [MemoryAddress("[[[[CurrentRuleset]+0xA8]+0x10]+0x4]")]
    public class KeyOverlay
    {
        [MemoryAddress("[+0x8]+0x1C")]
        public bool K1Pressed { get; set; }
        [MemoryAddress("[+0x8]+0x14")]
        public int K1Count { get; set; }
        [MemoryAddress("[+0xC] + 0x1C")]
        public bool K2Pressed { get; set; }
        [MemoryAddress("[+0xC] + 0x14")]
        public int K2Count { get; set; }
        [MemoryAddress("[+0x10] + 0x1C")]
        public bool M1Pressed { get; set; }
        [MemoryAddress("[+0x10] + 0x14")]
        public int M1Count { get; set; }
        [MemoryAddress("[+0x14] + 0x1C")]
        public bool M2Pressed { get; set; }
        [MemoryAddress("[+0x14] + 0x14")]
        public int M2Count { get; set; }

    }
}