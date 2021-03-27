using System;
using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.OsuMemoryModels.Abstract
{
    public class GeneralData
    {
        [MemoryAddress("OsuStatus")]
        public int RawStatus { get; set; }
        [MemoryAddress("GameMode")]
        public int GameMode { get; set; }
        [MemoryAddress("Retries")]
        public int Retries { get; set; }
        [MemoryAddress("AudioTime")]
        public int AudioTime { get; set; }
        [MemoryAddress("ChatIsExpanded")]
        public bool ChatIsExpanded { get; set; }
        [MemoryAddress("Mods")]
        public int Mods { get; set; }
        public OsuMemoryStatus OsuStatus
        {
            get
            {
                if (Enum.IsDefined(typeof(OsuMemoryStatus), RawStatus))
                {
                    return (OsuMemoryStatus)RawStatus;
                }

                return OsuMemoryStatus.Unknown;
            }
        }
    }
}