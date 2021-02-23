using OsuMemoryDataProvider.Models.Memory;
using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.Models
{
    public class MultiplayerPlayer
    {
        [MemoryAddress("+0x8")]
        public string Username { get; set; }
        [MemoryAddress("+0x30")]
        public int Score { get; set; }
        [MemoryAddress("[+0x20]+0x94")]
        public ushort Combo { get; set; }
        [MemoryAddress("[+0x20]+0x68")]
        public ushort MaxCombo { get; set; }

        [MemoryAddress("[+0x20]")]
        public Mods Mods { get; set; } = new Mods();

        [MemoryAddress("[+0x20]+0x8A")]
        public ushort Hit300 { get; set; }
        [MemoryAddress("[+0x20]+0x88")]
        public ushort Hit100 { get; set; }
        [MemoryAddress("[+0x20]+0x8C")]
        public ushort Hit50 { get; set; }
        [MemoryAddress("[+0x20]+0x92")]
        public ushort HitMiss { get; set; }
        [MemoryAddress("+0x40")]
        public int Team { get; set; }
        [MemoryAddress("+0x2C")]
        public int Position { get; set; }
        [MemoryAddress("+0x4B")]
        public bool IsPassing { get; set; }
        [MemoryAddress("[+0x24]+0x20")]
        public bool IsLeaderboardVisible { get; set; }
    }
}