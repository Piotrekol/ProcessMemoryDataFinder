using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.OsuMemoryModels.Direct
{
    [MemoryAddress("[CurrentRuleset]")]
    public class ResultsScreen : RulesetPlayData
    {
        [MemoryAddress("[+0x38]+0x78")]
        public override int Score { get; set; }
    }
}