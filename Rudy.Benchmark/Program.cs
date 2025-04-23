using BenchmarkDotNet.Running;
using Rudy.Benchmark.Benchmarks;

BenchmarkRunner.Run<SetGetBenchmark>();
// Uncomment to run pub/sub too:
// BenchmarkRunner.Run<PubSubBenchmark>();