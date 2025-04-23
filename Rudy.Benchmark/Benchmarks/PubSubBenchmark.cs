using BenchmarkDotNet.Attributes;

namespace Rudy.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class PubSubBenchmark
{
    private RudyClient _publisher = null!;
    private RudyClient _subscriber = null!;
    private string _channel = "bench";

    [GlobalSetup]
    public async Task Setup()
    {
        _publisher = new RudyClient("127.0.0.1", 6379);
        _subscriber = new RudyClient("127.0.0.1", 6379);

        await _subscriber.SendAsync($"SUBSCRIBE {_channel}");
        _ = Task.Run(async () =>
        {
            while (await _subscriber.ReceiveAsync() is { } line)
            {
                Console.WriteLine(line);
            }
        });
    }

    [Benchmark]
    public async Task Publish()
    {
        await _publisher.SendAsync($"PUBLISH {_channel} hello");
        await _publisher.ReceiveAsync();
    }
}