using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.OsuMemoryModels.Direct
{
    [MemoryAddress("[CurrentBeatmap]")]
    public class CurrentBeatmap
    {
        [MemoryAddress("+0xC4")]
        public int Id { get; set; }
        [MemoryAddress("+0xC8")]
        public int SetId { get; set; }
        [MemoryAddress("+0x7C")]
        public string MapString { get; set; }
        [MemoryAddress("+0x74")]
        public string FolderName { get; set; }
        [MemoryAddress("+0x8C")]
        public string OsuFileName { get; set; }
        [MemoryAddress("+0x6C")]
        public string Md5 { get; set; }
        [MemoryAddress("+0x2C")]
        public float Ar { get; set; }
        [MemoryAddress("+0x30")]
        public float Cs { get; set; }
        [MemoryAddress("+0x34")]
        public float Hp { get; set; }
        [MemoryAddress("+0x38")]
        public float Od { get; set; }
    }
}