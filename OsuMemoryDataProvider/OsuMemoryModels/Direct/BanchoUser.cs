using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.OsuMemoryModels.Direct
{
    [MemoryAddress("[UserPanel]")]
    public class BanchoUser
    {
        [MemoryAddress("+0x30")] public string Username { get; set; }
        [MemoryAddress("+0x70")] public int? UserId { get; set; }
        [MemoryAddress("+0x2C")] public string UserCountry { get; set; }
        [MemoryAddress("+0x1C")] public string UserPpAccLevel { get; set; }
        //[MemoryAddress("+0x74")] public float? UserLevel { get; set; }
        [MemoryAddress("+0x88")] public int? RawBanchoStatus { get; set; }
        public BanchoStatus? BanchoStatus => (BanchoStatus)(RawBanchoStatus ?? null);
    }
}
