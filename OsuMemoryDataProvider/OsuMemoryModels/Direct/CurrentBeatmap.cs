using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.OsuMemoryModels.Direct
{
    [MemoryAddress("[CurrentBeatmap]", false, true)]
    public class CurrentBeatmap
    {
        [MemoryAddress("+0xCC")]
        public int Id { get; set; }
        [MemoryAddress("+0xD0")]
        public int SetId { get; set; }
        [MemoryAddress("+0x80")]
        public string MapString { get; set; }
        [MemoryAddress("+0x78")]
        public string FolderName { get; set; }
        [MemoryAddress("+0x94")]
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
        [MemoryAddress("+0x130")]
        public short Status { get; set; }
    }
}