using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.Models
{
    [MemoryAddress("[CurrentBeatmap]")]
    public class CurrentBeatmap
    {
        [MemoryAddress("+0xC4")]
        public int MapId { get; set; }
        [MemoryAddress("+0xC8")]
        public int MapSetId { get; set; }
        [MemoryAddress("+0x7C")]
        public string MapString { get; set; }
        [MemoryAddress("+0x74")]
        public string MapFolderName { get; set; }
        [MemoryAddress("+0x8C")]
        public string MapOsuFileName { get; set; }
        [MemoryAddress("+0x6C")]
        public string MapMd5 { get; set; }
        [MemoryAddress("+0x2C")]
        public float MapAr { get; set; }
        [MemoryAddress("+0x30")]
        public float MapCs { get; set; }
        [MemoryAddress("+0x34")]
        public float MapHp { get; set; }
        [MemoryAddress("+0x38")]
        public float MapOd { get; set; }
    }
}