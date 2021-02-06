using System;
using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.Models
{
    public class Misc
    {
        [MemoryAddress("OsuStatus")]
        public int RawStatus { get; set; }

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