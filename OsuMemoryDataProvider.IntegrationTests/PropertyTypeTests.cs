using AwesomeAssertions;
using OsuMemoryDataProvider.IntegrationTests.TestHelpers;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;
using System.Collections.Generic;
using Xunit;

namespace OsuMemoryDataProvider.IntegrationTests;

[Collection(TestCollection.Name)]
public class PropertyTypeTests
{
    private readonly TestFixture _fixture;

    public PropertyTypeTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ReadInt_Succeeds()
    {
        CurrentBeatmap beatmap = new();
        bool result = _fixture.Reader.TryReadProperty(beatmap, nameof(CurrentBeatmap.Id), out object value);

        Assert.True(result);
        _ = Assert.IsType<int>(value);
        Assert.True((int)value > 0);
    }

    [Fact]
    public void ReadShort_Succeeds()
    {
        CurrentBeatmap beatmap = new();
        bool result = _fixture.Reader.TryReadProperty(beatmap, nameof(CurrentBeatmap.Status), out object value);

        Assert.True(result);
        _ = Assert.IsType<short>(value);
    }

    [Fact]
    public void ReadFloat_Succeeds()
    {
        CurrentBeatmap beatmap = new();
        bool result = _fixture.Reader.TryReadProperty(beatmap, nameof(CurrentBeatmap.Ar), out object value);

        Assert.True(result);
        _ = Assert.IsType<float>(value);
        Assert.True((float)value is >= 0 and <= 10);
    }

    [Fact]
    public void ReadDouble_Succeeds()
    {
        GeneralData generalData = new();
        bool result = _fixture.Reader.TryReadProperty(generalData, nameof(GeneralData.TotalAudioTime), out object value);

        Assert.True(result);
        double doubleValue = Assert.IsType<double>(value);
        _ = doubleValue.Should().BeInRange(0, 500_000);
    }

    [Fact]
    public void ReadBool_Succeeds()
    {
        GeneralData generalData = new();
        bool result = _fixture.Reader.TryReadProperty(generalData, nameof(GeneralData.ChatIsExpanded), out object value);

        Assert.True(result);
        _ = Assert.IsType<bool>(value);
    }

    [Fact]
    public void ReadString_Succeeds()
    {
        CurrentBeatmap beatmap = new();
        bool result = _fixture.Reader.TryReadProperty(beatmap, nameof(CurrentBeatmap.MapString), out object value);

        Assert.True(result);
        _ = Assert.IsType<string>(value);
        Assert.NotNull(value);
        Assert.NotEmpty((string)value);
    }

    [Fact]
    public void ReadNullableInt_Succeeds()
    {
        SongSelectionScores songSelectionScores = new();
        bool result = _fixture.Reader.TryReadProperty(songSelectionScores, nameof(SongSelectionScores.AmountOfScores), out object value);

        Assert.True(result);
        _ = Assert.IsType<int>(value);
    }

    [Fact]
    public void ReadCustomObject_Succeeds()
    {
        Player player = new();
        _ = _fixture.Reader.TryRead(player);

        Assert.NotNull(player.Mods);
        _ = Assert.IsType<Mods>(player.Mods);
    }

    [Fact]
    public void ReadListInt_Succeeds()
    {
        Player player = new();
        _ = _fixture.Reader.TryRead(player);

        if (player.HitErrors != null)
        {
            _ = Assert.IsType<System.Collections.Generic.List<int>>(player.HitErrors);
            Assert.Empty(player.HitErrors);
        }
    }

    [Fact]
    public void ReadListPlayerScore_Succeeds()
    {
        SongSelectionScores songSelectionScores = new();
        _ = _fixture.Reader.TryRead(songSelectionScores);
        List<PlayerScore> scores = songSelectionScores.Scores;

        Assert.NotNull(scores);
        _ = Assert.IsType<System.Collections.Generic.List<PlayerScore>>(scores);
        Assert.True(scores.Count > 0);
    }

    [Fact]
    public void ReadListMultiplayerPlayer_Succeeds()
    {
        LeaderBoard leaderboard = new();
        _ = _fixture.Reader.TryRead(leaderboard);
        List<MultiplayerPlayer> players = leaderboard.Players;

        Assert.NotNull(players);
        _ = Assert.IsType<System.Collections.Generic.List<MultiplayerPlayer>>(players);
    }
}
