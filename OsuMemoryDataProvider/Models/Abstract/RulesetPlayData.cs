using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.Models
{
    public abstract class RulesetPlayData
    {
        [MemoryAddress("[+0x38]+0x28")]
        public string Username { get; set; }
        [MemoryAddress("[[+0x38]+0x1C]+0xC")]
        private int ModsXor1 { get; set; }
        [MemoryAddress("[[+0x38]+0x1C]+0x8")]
        private int ModsXor2 { get; set; }
        public int Mods => ModsXor1 ^ ModsXor2;

        [MemoryAddress("[+0x38]+0x64")]
        public int Mode { get; set; }
        [MemoryAddress("[+0x38]+0x68")]
        public ushort MaxCombo { get; set; }
        [MemoryAddress("[+0x38]+0x78")]
        public int Score { get; set; }
        [MemoryAddress("[+0x38]+0x88")]
        public ushort Hit100 { get; set; }
        [MemoryAddress("[+0x38]+0x8A")]
        public ushort Hit300 { get; set; }
        [MemoryAddress("[+0x38]+0x8C")]
        public ushort Hit50 { get; set; }
        [MemoryAddress("[+0x38]+0x8E")]
        public ushort HitGeki { get; set; }
        [MemoryAddress("[+0x38]+0x90")]
        public ushort HitKatu { get; set; }
        [MemoryAddress("[+0x38]+0x92")]
        public ushort HitMiss { get; set; }
        [MemoryAddress("[+0x38]+0x94")]
        public ushort Combo { get; set; }

    }
}