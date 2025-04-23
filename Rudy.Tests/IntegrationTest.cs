using System.Diagnostics;
using System.Net.Sockets;
using Rudy.Server;
using Rudy.Server.Builders;
using Rudy.Server.Stores;
using Xunit.Abstractions;

namespace Rudy.Tests;

public class IntegrationTest(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData(6390, 6)]
    public async Task RedisClone_FullFlow_WithReplicasAndPubSub_ShouldSyncAndBroadcastCorrectly(int masterPort, int replicaNumber)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        
        var masterServer = RudyServerBuilder.Initialize().WithPort(masterPort).Build();
        _ = Task.Run(() => masterServer.StartAsync(cancellationTokenSource.Token), cancellationTokenSource.Token);
        await Task.Delay(200, cancellationTokenSource.Token);

        // Setup 3 replicas
        var replicas = new List<MemoryStore>();
        for (var i = 0; i < replicaNumber; i++)
        {
            var replica = MemoryStore.Create();
            replicas.Add(replica);

            _ = Task.Run(async () =>
            {
                var replicaSocket = new TcpClient();
                await replicaSocket.ConnectAsync("127.0.0.1", masterPort, cancellationTokenSource.Token);
                var writer = new StreamWriter(replicaSocket.GetStream()) { AutoFlush = true };
                await writer.WriteLineAsync("REPLICA");

                var reader = new StreamReader(replicaSocket.GetStream());
                while (true)
                {
                    var line = await reader.ReadLineAsync(cancellationTokenSource.Token);
                    if (line == null) break;

                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3 && parts[0].ToUpper() == "SET")
                        replica.Set(parts[1], parts[2]);
                    else if (parts[0].ToUpper() == "DEL" && parts.Length >= 2)
                        replica.Delete(parts[1]);
                }
            }, cancellationTokenSource.Token);
        }

        await Task.Delay(300, cancellationTokenSource.Token);

        // Setup subscriber client
        var subscriberClient = new TcpClient();
        await subscriberClient.ConnectAsync("127.0.0.1", masterPort, cancellationTokenSource.Token);
        var subWriter = new StreamWriter(subscriberClient.GetStream()) { AutoFlush = true };
        var subReader = new StreamReader(subscriberClient.GetStream());
        await subWriter.WriteLineAsync("SUBSCRIBE news");
        await subReader.ReadLineAsync(cancellationTokenSource.Token); // Subscribed

        // Setup publisher client
        var pubClient = new TcpClient();
        await pubClient.ConnectAsync("127.0.0.1", masterPort, cancellationTokenSource.Token);
        var pubWriter = new StreamWriter(pubClient.GetStream()) { AutoFlush = true };
        var pubReader = new StreamReader(pubClient.GetStream());

        // Publish a message
        const string testMessage = "hello-world";
        await pubWriter.WriteLineAsync($"PUBLISH news {testMessage}");
        var pubAck = await pubReader.ReadLineAsync(cancellationTokenSource.Token);
        Assert.Contains("Delivered", pubAck);

        var receivedMsg = await subReader.ReadLineAsync(cancellationTokenSource.Token);
        Assert.Contains($"message news {testMessage}", receivedMsg);

        // Benchmark SET commands
        var benchClient = new TcpClient();
        await benchClient.ConnectAsync("127.0.0.1", masterPort, cancellationTokenSource.Token);
        var benchWriter = new StreamWriter(benchClient.GetStream()) { AutoFlush = true };
        var benchReader = new StreamReader(benchClient.GetStream());

        const int ops = 1000;
        var sw = Stopwatch.StartNew();

        for (var i = 0; i < ops; i++)
        {
            await benchWriter.WriteLineAsync($"SET key{i} val{i}");
            await benchReader.ReadLineAsync(cancellationTokenSource.Token); // OK
        }

        sw.Stop();
        var opsPerSecond = ops / sw.Elapsed.TotalSeconds;

        testOutputHelper.WriteLine($"Benchmark: {ops} SETs in {sw.Elapsed.TotalSeconds:F2}s = {opsPerSecond:F0} ops/sec");

        // Validate replication state
        foreach (var val in replicas.Select(replica => replica.Get("key999")))
        {
            Assert.Equal("val999", val);
        }
    }
}