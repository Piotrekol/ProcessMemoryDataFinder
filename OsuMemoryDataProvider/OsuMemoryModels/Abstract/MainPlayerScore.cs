using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.OsuMemoryModels.Abstract
{
    public class MainPlayerScore : PlayerScore
    {
        [MemoryAddress("+0x6C")] public int Position { get; set; }
    }
}