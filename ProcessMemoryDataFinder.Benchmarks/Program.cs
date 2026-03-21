using BenchmarkDotNet.Running;
using ProcessMemoryDataFinder.Benchmarks;

// Run benchmarks
// IMPORTANT: osu! must be running for these benchmarks to work
BenchmarkRunner.Run<MemoryReadBenchmarks>();
