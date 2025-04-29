using System.Diagnostics;
using System.Net.Sockets;
using Rudy.Server;
using Rudy.Server.Builders;
using Xunit.Abstractions;

namespace Rudy.Tests;

public class IntegrationTest(ITestOutputHelper output) : IAsyncLifetime
{
    private readonly List<RudyServer> _replicaServers = new();
    private RudyServer? _masterServer;
    private readonly CancellationTokenSource _cts = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _masterServer?.Stop();
        foreach (var replica in _replicaServers) 
            replica.Stop();
        return Task.CompletedTask;
    }

    [Theory]
    [InlineData(6390, 3)]
    public async Task RudyServer_FullFlow_WithRealReplicas_ShouldSyncAndBenchmarkCorrectly(int masterPort, int replicaCount)
    {
        _masterServer = RudyServerBuilder.Initialize($"{masterPort}.log")
            .WithPort(masterPort)
            .Build();
        _ = Task.Run(() => _masterServer.Start(), _cts.Token);
        await Task.Delay(500);

        for (var i = 0; i < replicaCount; i++)
        {
            var replicaPort = masterPort + i + 1;

            var replica = RudyServerBuilder.Initialize($"{replicaPort}.log")
                .WithPort(replicaPort)
                .Build();

            _replicaServers.Add(replica);
            _ = Task.Run(() => replica.Start(), _cts.Token);
            
            await replica.ConnectAsReplicaAsync("127.0.0.1", masterPort);
        }

        var subscriber = new TcpClient();
        await subscriber.ConnectAsync("127.0.0.1", masterPort, _cts.Token);
        var subWriter = new StreamWriter(subscriber.GetStream()) { AutoFlush = true };
        var subReader = new StreamReader(subscriber.GetStream());
        await subWriter.WriteLineAsync("SUBSCRIBE news");
        await subReader.ReadLineAsync(_cts.Token); // OK subscribed

        var publisher = new TcpClient();
        await publisher.ConnectAsync("127.0.0.1", masterPort, _cts.Token);
        var pubWriter = new StreamWriter(publisher.GetStream()) { AutoFlush = true };
        var pubReader = new StreamReader(publisher.GetStream());

        const string testMessage = "hello-world";
        await pubWriter.WriteLineAsync($"PUBLISH news {testMessage}");
        var pubAck = await pubReader.ReadLineAsync(_cts.Token);
        Assert.Contains("Delivered", pubAck);

        var received = await subReader.ReadLineAsync(_cts.Token);
        Assert.Contains($"message news {testMessage}", received);

        var benchClient = new TcpClient();
        await benchClient.ConnectAsync("127.0.0.1", masterPort, _cts.Token);
        var benchWriter = new StreamWriter(benchClient.GetStream()) { AutoFlush = true };
        var benchReader = new StreamReader(benchClient.GetStream());

        const int ops = 1000;
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < ops; i++)
        {
            await benchWriter.WriteLineAsync($"SET key{i} val{i}");
            await benchReader.ReadLineAsync(_cts.Token); // OK
        }
        sw.Stop();
        var opsPerSec = ops / sw.Elapsed.TotalSeconds;
        output.WriteLine($"Benchmark: {ops} SETs in {sw.Elapsed.TotalSeconds:F2}s = {opsPerSec:F0} ops/sec");

        await Task.Delay(500);

        foreach (var replica in _replicaServers)
        {
            var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", replica.Port, _cts.Token);
            var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
            var reader = new StreamReader(client.GetStream());

            await writer.WriteLineAsync("GET key999");
            var value = await reader.ReadLineAsync(_cts.Token);

            Assert.Equal("val999", value);
        }
    }
}