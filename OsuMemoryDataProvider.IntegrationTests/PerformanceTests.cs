using OsuMemoryDataProvider.IntegrationTests.TestHelpers;
using OsuMemoryDataProvider.OsuMemoryModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OsuMemoryDataProvider.IntegrationTests;

[Collection("Integration Tests")]
public class PerformanceTests
{
    private readonly ITestOutputHelper _output;
    private readonly StructuredOsuMemoryReader _reader;
    private readonly TimeSpan _testDuration = TimeSpan.FromSeconds(2);
    private readonly TimeSpan _readDelay = TimeSpan.FromMilliseconds(33);

    public PerformanceTests(ITestOutputHelper output, TestFixture fixture)
    {
        _output = output;
        _reader = fixture.Reader;
    }

    [Fact]
    public async Task Performance_ReadBaseAddresses_MeasuresTiming()
    {
        if (!_reader.OsuAvaliable(_output))
        {
            return;
        }

        // Warmup
        OsuBaseAddresses baseAddresses = new();
        const int warmupCalls = 100;
        for (int i = 0; i < warmupCalls; i++)
        {
            _reader.ReadAll(baseAddresses);

            await Task.Delay(_readDelay);
        }

        // Measured runs 
        Stopwatch stopwatch = new();
        List<double> readTimes = [];

        int iterations = 0;
        DateTimeOffset startedAt = DateTimeOffset.UtcNow;
        DateTimeOffset endsAt = startedAt.Add(_testDuration);
        stopwatch.Start();

        while (DateTimeOffset.UtcNow < endsAt)
        {
            stopwatch.Restart();

            _reader.ReadAll(baseAddresses);

            double readTimeMs = stopwatch.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond;
            readTimes.Add(readTimeMs);
            iterations++;
            await Task.Delay(_readDelay);
        }

        stopwatch.Stop();

        double minTime = readTimes.Min();
        double maxTime = readTimes.Max();
        double avgTime = readTimes.Average();

        _output.WriteLine($"Performance Test Results:");
        _output.WriteLine($"  Total Duration: {_testDuration.TotalSeconds:F2}s");
        _output.WriteLine($"  Warmup iterations: {warmupCalls}");
        _output.WriteLine($"  Measured iterations: {iterations}");
        _output.WriteLine($"  Delay between reads: {_readDelay.TotalMilliseconds}ms");
        _output.WriteLine($"  Min Read Time: {minTime:F2}ms");
        _output.WriteLine($"  Max Read Time: {maxTime:F2}ms");
        _output.WriteLine($"  Avg Read Time: {avgTime:F2}ms");

    }
}
