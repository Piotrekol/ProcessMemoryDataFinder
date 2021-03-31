using System;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;
using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.OsuMemoryModels.Abstract
{
    public class GeneralData
    {
        [MemoryAddress("OsuStatus")] public int RawStatus { get; set; }
        [MemoryAddress("GameMode")] public int GameMode { get; set; }
        [MemoryAddress("Retries")] public int Retries { get; set; }
        [MemoryAddress("AudioTime")] public int AudioTime { get; set; }
        [MemoryAddress("ChatIsExpanded")] public bool ChatIsExpanded { get; set; }
        [MemoryAddress("Mods")] public int Mods { get; set; }

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

        [MemoryAddress(KeyOverlay.ClassAddress)]
        private int? KeyOverlayAddress { get; set; }

        private bool KeyOverlayExists => KeyOverlayAddress.HasValue && KeyOverlayAddress != 0;
        private KeyOverlay _keyOverlay { get; set; } = new KeyOverlay();

        [MemoryAddress(null)]
        public KeyOverlay KeyOverlay
        {
            get => KeyOverlayExists ? _keyOverlay : null;
            set => _keyOverlay = value;
        }
    }
}