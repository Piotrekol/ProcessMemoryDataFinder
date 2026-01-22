using OsuMemoryDataProvider.OsuMemoryModels;
using System;
using Xunit.Abstractions;

namespace OsuMemoryDataProvider.IntegrationTests.TestHelpers;

internal static class StructuredOsuMemoryReaderExtensions
{

    public static bool OsuAvaliable(this StructuredOsuMemoryReader reader, ITestOutputHelper output)
    {
        DateTimeOffset stopAt = DateTimeOffset.UtcNow.AddSeconds(2);

        while (true)
        {
            if (reader.CanRead)
            {
                break;
            }
            else if (DateTimeOffset.UtcNow > stopAt)
            {
                output.WriteLine("[SKIP] osu! process is not running. Skipping performance test.");
                return false;
            }
        }

        return true;
    }

    public static void ReadAll(this StructuredOsuMemoryReader _reader, OsuBaseAddresses baseAddresses)
    {
        _ = _reader.TryRead(baseAddresses.Beatmap);
        _ = _reader.TryRead(baseAddresses.Player);
        _ = _reader.TryRead(baseAddresses.LeaderBoard);
        _ = _reader.TryRead(baseAddresses.SongSelectionScores);
        _ = _reader.TryRead(baseAddresses.Skin);
        _ = _reader.TryRead(baseAddresses.ResultsScreen);
        _ = _reader.TryRead(baseAddresses.GeneralData);
        _ = _reader.TryRead(baseAddresses.BanchoUser);
        _ = _reader.TryRead(baseAddresses.KeyOverlay);
    }
}