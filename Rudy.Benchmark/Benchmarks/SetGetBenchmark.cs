using BenchmarkDotNet.Attributes;

namespace Rudy.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class SetGetBenchmark
{
    private RudyClient _client = null!;

    [GlobalSetup]
    public void Setup()
    {
        _client = new RudyClient("127.0.0.1", 6379);
    }

    [Benchmark]
    public async Task Set()
    {
        await _client.SendAsync($"SET key123 value123");
        await _client.ReceiveAsync();
    }

    [Benchmark]
    public async Task Get()
    {
        await _client.SendAsync($"GET key123");
        await _client.ReceiveAsync();
    }
}