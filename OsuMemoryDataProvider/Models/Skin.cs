using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.Models
{
    [MemoryAddress("[CurrentSkinData]")]
    public class Skin
    {
        [MemoryAddress("+0x44")]
        public string Folder { get; set; }
    }
}