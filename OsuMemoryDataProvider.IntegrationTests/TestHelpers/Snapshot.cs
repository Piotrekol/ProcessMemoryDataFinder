using Newtonsoft.Json;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;
using System;

namespace OsuMemoryDataProvider.IntegrationTests.TestHelpers;

public record SnapshotMetadata
{
    public DateTime Timestamp { get; set; }
    public string ProcessName { get; set; }
    public string WindowTitleHint { get; set; }
    public int Bitness { get; set; }
    public string GameState { get; set; }
}

public class Snapshot
{
    public SnapshotMetadata Metadata { get; set; }

    [JsonProperty("beatmap")]
    public CurrentBeatmap Beatmap { get; set; }

    [JsonProperty("player")]
    public Player Player { get; set; }

    [JsonProperty("leaderBoard")]
    public LeaderBoard LeaderBoard { get; set; }

    [JsonProperty("songSelectionScores")]
    public SongSelectionScores SongSelectionScores { get; set; }

    [JsonProperty("skin")]
    public Skin Skin { get; set; }

    [JsonProperty("resultsScreen")]
    public ResultsScreen ResultsScreen { get; set; }

    [JsonProperty("generalData")]
    public GeneralData GeneralData { get; set; }

    [JsonProperty("banchoUser")]
    public BanchoUser BanchoUser { get; set; }

    [JsonProperty("keyOverlay")]
    public KeyOverlay KeyOverlay { get; set; }
}
