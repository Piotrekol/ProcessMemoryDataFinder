using AwesomeAssertions;
using Newtonsoft.Json;
using OsuMemoryDataProvider.IntegrationTests.TestHelpers;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using ProcessMemoryDataFinder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace OsuMemoryDataProvider.IntegrationTests;

[Collection(TestCollection.Name)]
public class SnapshotTests
{
    private readonly ITestOutputHelper _output;
    private readonly StructuredOsuMemoryReader _reader;
    private readonly TestConfiguration _configuration;
    private readonly string _snapshotDirectory;
    private readonly string _snapshotFilePath;

    public SnapshotTests(ITestOutputHelper output, TestFixture fixture)
    {
        _output = output;
        _configuration = new TestConfiguration();
        ProcessTargetOptions options = _configuration.ToProcessTargetOptions();
        _reader = StructuredOsuMemoryReader.GetInstance(options);
        _snapshotDirectory = Path.Combine(AppContext.BaseDirectory, "Snapshots");
        _snapshotFilePath = Path.Combine(_snapshotDirectory, "osu_snapshot_baseline.json");
    }

    [Fact]
    public void Snapshot_WhenOfflineOnMap5248423_NoChanges()
    {
        if (!_reader.OsuAvaliable(_output))
        {
            return;
        }

        if (!File.Exists(_snapshotFilePath))
        {
            _ = Directory.CreateDirectory(_snapshotDirectory);

            Snapshot baseline = CreateSnapshot();
            string json = JsonConvert.SerializeObject(baseline, Formatting.Indented);
            File.WriteAllText(_snapshotFilePath, json);

            _output.WriteLine($"[BASELINE CREATED] Saved baseline snapshot to: {_snapshotFilePath}");
            _output.WriteLine($"[BASELINE CREATED] Run this test again to verify no regressions.");
            _output.WriteLine($"[BASELINE CREATED] Path used: {Path.Combine(AppContext.BaseDirectory, "Snapshots")}");
            return;
        }

        string baselineJson = File.ReadAllText(_snapshotFilePath);
        Snapshot baselineSnapshot = JsonConvert.DeserializeObject<Snapshot>(baselineJson);
        Snapshot currentSnapshot = CreateSnapshot();
        _ = baselineSnapshot
            .GeneralData
            .Should()
            .BeEquivalentTo(
                currentSnapshot.GeneralData,
                options => options
                    .Excluding(s => s.AudioTime));

        _ = baselineSnapshot
            .Beatmap
            .Should()
            .BeEquivalentTo(currentSnapshot.Beatmap);

        int readScoresCount = currentSnapshot.SongSelectionScores.AmountOfScores.Value;
        List<PlayerScore> readScores = currentSnapshot.SongSelectionScores.Scores.Take(readScoresCount).ToList();
        List<PlayerScore> snapshotScores = baselineSnapshot.SongSelectionScores.Scores.Skip(readScoresCount).ToList();
        _ = snapshotScores
            .Should()
            .BeEquivalentTo(
                readScores,
                options => options
                    .Excluding(s => s.Mods.ModsXor1)
                    .Excluding(s => s.Mods.ModsXor2)
                    .Excluding(s => s.Date));
    }

    private Snapshot CreateSnapshot()
    {
        OsuBaseAddresses baseAddresses = new();

        _reader.ReadAll(baseAddresses);
        _reader.ReadAll(baseAddresses);
        _reader.ReadAll(baseAddresses);
        _reader.ReadAll(baseAddresses);
        _reader.ReadAll(baseAddresses);
        _reader.ReadAll(baseAddresses);

        return new Snapshot
        {
            Metadata = new SnapshotMetadata
            {
                Timestamp = DateTime.UtcNow,
                ProcessName = _configuration.ProcessName,
                WindowTitleHint = _configuration.WindowTitleHint,
                Bitness = Environment.Is64BitProcess ? 64 : 32,
                GameState = baseAddresses.GeneralData.OsuStatus.ToString()
            },
            Beatmap = baseAddresses.Beatmap,
            Player = baseAddresses.Player,
            LeaderBoard = baseAddresses.LeaderBoard,
            SongSelectionScores = baseAddresses.SongSelectionScores,
            Skin = baseAddresses.Skin,
            ResultsScreen = baseAddresses.ResultsScreen,
            GeneralData = baseAddresses.GeneralData,
            BanchoUser = baseAddresses.BanchoUser,
            KeyOverlay = baseAddresses.KeyOverlay
        };
    }
}
