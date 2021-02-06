using System.Collections.Generic;
using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.Models
{
    [MemoryAddress("[[CurrentRuleset] + 0x60]")]
    public class Play : RulesetPlayData
    {
        //[MemoryAddress("")]
        //public int Retries { get; set; }

        [MemoryAddress("[+0x40]+0x14")]
        public double PlayerHPSmooth { get; set; }
        [MemoryAddress("[+0x40]+0x1C")]
        public double PlayerHP { get; set; }
        [MemoryAddress("[+0x48]+0xC")]
        public double Accuracy { get; set; }
        [MemoryAddress("[+0x38]+0x38")]
        public List<int> HitErrors { get; set; }

        [MemoryAddress("IsReplay")]
        public bool IsReplay { get; set; }
    }
}