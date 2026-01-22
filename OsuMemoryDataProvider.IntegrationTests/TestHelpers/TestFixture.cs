using OsuMemoryDataProvider.OsuMemoryModels;
using ProcessMemoryDataFinder;
using Xunit;

namespace OsuMemoryDataProvider.IntegrationTests.TestHelpers;

[CollectionDefinition(Name)]
public class TestCollection : ICollectionFixture<TestFixture>
{
    public const string Name = "Integration Tests";
}

public class TestFixture
{
    public StructuredOsuMemoryReader Reader { get; }
    private readonly TestConfiguration _configuration;
    public ProcessTargetOptions Options { get; }

    public TestFixture()
    {
        _configuration = new TestConfiguration();
        Options = _configuration.ToProcessTargetOptions();
        Reader = StructuredOsuMemoryReader.GetInstance(Options);
    }

    public OsuBaseAddresses GetFreshAddresses() => new();

    public OsuBaseAddresses ReadAllAddresses()
    {
        OsuBaseAddresses baseAddresses = GetFreshAddresses();

        _ = Reader.TryRead(baseAddresses.Beatmap);
        _ = Reader.TryRead(baseAddresses.Player);
        _ = Reader.TryRead(baseAddresses.LeaderBoard);
        _ = Reader.TryRead(baseAddresses.SongSelectionScores);
        _ = Reader.TryRead(baseAddresses.Skin);
        _ = Reader.TryRead(baseAddresses.ResultsScreen);
        _ = Reader.TryRead(baseAddresses.GeneralData);
        _ = Reader.TryRead(baseAddresses.BanchoUser);
        _ = Reader.TryRead(baseAddresses.KeyOverlay);

        return baseAddresses;
    }
}
