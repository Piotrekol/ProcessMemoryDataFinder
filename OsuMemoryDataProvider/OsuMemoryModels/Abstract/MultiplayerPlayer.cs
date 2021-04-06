using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.OsuMemoryModels.Abstract
{
    public class MultiplayerPlayer
    {
        [MemoryAddress("+0x8")]
        public string Username { get; set; }
        [MemoryAddress("+0x30")]
        public int Score { get; set; }

        [MemoryAddress("[+0x20]")]
        private MultiplayerPlayerPlayData _multiplayerPlayerPlayData { get; set; } = new MultiplayerPlayerPlayData();

        public ushort Combo => _multiplayerPlayerPlayData?.Combo ?? 0;
        public ushort MaxCombo => _multiplayerPlayerPlayData?.MaxCombo ?? 0;
        public Mods Mods => _multiplayerPlayerPlayData?.Mods;
        public ushort Hit300 => _multiplayerPlayerPlayData?.Hit300 ?? 0;
        public ushort Hit100 => _multiplayerPlayerPlayData?.Hit100 ?? 0;
        public ushort Hit50 => _multiplayerPlayerPlayData?.Hit50 ?? 0;
        public ushort HitMiss => _multiplayerPlayerPlayData?.HitMiss ?? 0;
        [MemoryAddress("+0x40")]
        public int Team { get; set; }
        [MemoryAddress("+0x2C")]
        public int Position { get; set; }
        [MemoryAddress("+0x4B")]
        public bool IsPassing { get; set; }
    }
}