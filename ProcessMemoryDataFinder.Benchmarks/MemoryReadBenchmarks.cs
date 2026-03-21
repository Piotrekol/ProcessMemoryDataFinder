using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;

namespace ProcessMemoryDataFinder.Benchmarks;

/// <summary>
/// Benchmarks for memory read optimization.
/// 
/// IMPORTANT: These benchmarks require osu! to be running.
/// Run with: dotnet run -c Release
/// 
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class MemoryReadBenchmarks
{
    private StructuredOsuMemoryReader _reader = default!;
    private GeneralData _generalData = default!;
    private Player _player = default!;
    private CurrentBeatmap _beatmap = default!;
    private KeyOverlay _keyOverlay = default!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _reader = StructuredOsuMemoryReader.Instance;
        _generalData = new GeneralData();
        _player = new Player();
        _beatmap = new CurrentBeatmap();
        _keyOverlay = new KeyOverlay();

        Console.WriteLine("Waiting for osu! process...");
        DateTime timeout = DateTime.Now.AddSeconds(30);
        while (!_reader.CanRead && DateTime.Now < timeout)
        {
            Thread.Sleep(500);
        }

        if (!_reader.CanRead)
        {
            throw new InvalidOperationException("osu! process not found. Please start osu! before running benchmarks.");
        }

        Console.WriteLine("Connected to osu! process.");
    }

    [IterationSetup]
    public void IterationSetup()
    {
        // Reset data before each iteration
    }

    [Benchmark]
    [BenchmarkCategory("GeneralData")]
    public bool GeneralData_IndividualReads() =>
        //_reader.UseBatching = false;
        _reader.TryRead(_generalData);

    [Benchmark]
    [BenchmarkCategory("Player")]
    public bool Player_IndividualReads() =>
        //_reader.UseBatching = false;
        _reader.TryRead(_player);

    [Benchmark]
    [BenchmarkCategory("CurrentBeatmap")]
    public bool CurrentBeatmap_IndividualReads() =>
        //_reader.UseBatching = false;
        _reader.TryRead(_beatmap);

    [Benchmark]
    [BenchmarkCategory("KeyOverlay")]
    public bool KeyOverlay_IndividualReads() =>
        //_reader.UseBatching = false;
        _reader.TryRead(_keyOverlay);

    [Benchmark]
    [BenchmarkCategory("Combined")]
    public bool Combined_IndividualReads()
    {
        bool success = _reader.TryRead(_generalData);
        success &= _reader.TryRead(_player);
        success &= _reader.TryRead(_beatmap);
        return success;
    }
}
