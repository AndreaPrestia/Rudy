using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Rudy.Server;
using Rudy.Server.Builders;
using Xunit.Abstractions;

namespace Rudy.Tests;

public class IntegrationTests(ITestOutputHelper output) : IAsyncLifetime
{
    private readonly List<RudyServer> _replicaServers = [];
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
    [InlineData(6390, 3, 1000)]
    public async Task RudyServer_FullFlow_WithRealReplicas_ShouldSyncAndBenchmarkCorrectly(int masterPort, int replicaCount, int benchmarkOperations)
    {
        _masterServer = RudyServerBuilder.Initialize($"{masterPort}.log")
            .WithIpAddress(IPAddress.Loopback)
            .WithPort(masterPort)
            .Build();
        _ = Task.Run(() => _masterServer.Start(), _cts.Token);
        await Task.Delay(500);

        for (var i = 0; i < replicaCount; i++)
        {
            var replicaPort = masterPort + i + 1;

            var replica = RudyServerBuilder.Initialize($"{replicaPort}.log")
                .WithIpAddress(IPAddress.Loopback)
                .WithPort(replicaPort)
                .Build();

            _replicaServers.Add(replica);
            _ = Task.Run(() => replica.Start(), _cts.Token);
            
            await replica.ConnectAsReplicaAsync(_masterServer.Host, _masterServer.Port);
        }

        var subscriber = new TcpClient();
        await subscriber.ConnectAsync(_masterServer.Host, _masterServer.Port, _cts.Token);
        var subWriter = new StreamWriter(subscriber.GetStream()) { AutoFlush = true };
        var subReader = new StreamReader(subscriber.GetStream());
        await subWriter.WriteLineAsync("SUB news");
        await subReader.ReadLineAsync(_cts.Token);

        var publisher = new TcpClient();
        await publisher.ConnectAsync(_masterServer.Host, _masterServer.Port, _cts.Token);
        var pubWriter = new StreamWriter(publisher.GetStream()) { AutoFlush = true };
        var pubReader = new StreamReader(publisher.GetStream());

        const string testMessage = "hello-world";
        await pubWriter.WriteLineAsync($"PUB news {testMessage}");
        var pubAck = await pubReader.ReadLineAsync(_cts.Token);
        Assert.Contains("Delivered", pubAck);

        var received = await subReader.ReadLineAsync(_cts.Token);
        Assert.Contains($"message news {testMessage}", received);

        var benchClient = new TcpClient();
        await benchClient.ConnectAsync(_masterServer.Host, _masterServer.Port, _cts.Token);
        var benchWriter = new StreamWriter(benchClient.GetStream()) { AutoFlush = true };
        var benchReader = new StreamReader(benchClient.GetStream());

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < benchmarkOperations; i++)
        {
            await benchWriter.WriteLineAsync($"SET key{i} val{i}");
            await benchReader.ReadLineAsync(_cts.Token); // OK
        }
        sw.Stop();
        var opsPerSec = benchmarkOperations / sw.Elapsed.TotalSeconds;
        output.WriteLine($"Benchmark: {benchmarkOperations} SETs in {sw.Elapsed.TotalSeconds:F2}s = {opsPerSec:F0} ops/sec");

        await Task.Delay(500);

        foreach (var replica in _replicaServers)
        {
            var client = new TcpClient();
            await client.ConnectAsync(replica.Host, replica.Port, _cts.Token);
            var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
            var reader = new StreamReader(client.GetStream());

            await writer.WriteLineAsync("GET key999");
            var value = await reader.ReadLineAsync(_cts.Token);

            Assert.Equal("val999", value);
            
            await writer.WriteLineAsync("HEALTH");
            var healthValue = await reader.ReadLineAsync(_cts.Token);

            Assert.Equal("OK", healthValue);
        }
    }
}