using System.Net;
using System.Net.Sockets;
using Rudy.Server.Managers;
using Rudy.Server.Stores;

namespace Rudy.Server;

public class TcpServer
{
    private readonly CancellationTokenSource _cts = new();
    private readonly TcpListener _listener;
    private readonly ReplicaManager _replicaManager;
    private readonly PubSubManager _pubSubManager;
    private readonly DiskStore _diskStore;
    private readonly MemoryStore _memoryStore;

    internal TcpServer(IPAddress ipAddress, int port, ReplicaManager replicaManager, PubSubManager pubSubManager, DiskStore diskStore, MemoryStore memoryStore)
    {
        _replicaManager = replicaManager;
        _pubSubManager = pubSubManager;
        _diskStore = diskStore;
        _memoryStore = memoryStore;
        _listener = new TcpListener(ipAddress, port);
    }
    
    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine("Rudy server started on port " + ((IPEndPoint)_listener.LocalEndpoint).Port);

        while (!_cts.Token.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(_cts.Token);
            _ = Task.Run(() => HandleClientAsync(client), _cts.Token);
        }
    }
    
    public Task StopAsync()
    {
        _cts.Cancel();
        _listener.Stop();
        return Task.CompletedTask;
    }
    
    public Task ConnectAsReplicaAsync(string masterHost, int masterPort)
    {
        return _replicaManager.ConnectToMaster(masterHost, masterPort);
    }


   private async Task HandleClientAsync(TcpClient client)
    {
        var stream = client.GetStream();
        var reader = new StreamReader(stream);
        var writer = new StreamWriter(stream) { AutoFlush = true };

        var isReplica = false;

        while (true)
        {
            string? line;
            try
            {
                line = await reader.ReadLineAsync();
                if (line == null) break;
            }
            catch
            {
                break;
            }

            switch (isReplica)
            {
                case false when line.Equals("REPLICA_REGISTER", StringComparison.CurrentCultureIgnoreCase):
                    isReplica = true;
                    _replicaManager.RegisterReplica(client);
                    continue;
                case true:
                    continue; // Ignore commands from replicas
            }

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            var response = "ERR unknown command";

            switch (parts[0].ToUpper())
            {
                case "PING":
                    response = "PONG";
                    break;

                case "SET":
                    if (parts.Length >= 3)
                    {
                        string key = parts[1], val = parts[2];
                        TimeSpan? ttl = null;

                        if (parts.Length >= 5 && parts[3].ToUpper() == "EX")
                            ttl = TimeSpan.FromSeconds(int.Parse(parts[4]));

                        _memoryStore.Set(key, val, ttl);
                        _diskStore.Log(line);
                        _replicaManager.Broadcast(line);
                        response = "OK";
                    }
                    break;

                case "GET":
                    if (parts.Length >= 2)
                    {
                        var val = _memoryStore.Get(parts[1]);
                        response = val?.ToString() ?? "(nil)";
                    }
                    break;

                case "DEL":
                    if (parts.Length >= 2)
                    {
                        var removed = _memoryStore.Delete(parts[1]);
                        _diskStore.Log(line);
                        _replicaManager.Broadcast(line);
                        response = removed ? "1" : "0";
                    }
                    break;

                case "SUBSCRIBE":
                    if (parts.Length >= 2)
                    {
                        _pubSubManager.Subscribe(parts[1], client);
                        response = $"Subscribed to {parts[1]}";
                    }
                    break;

                case "PUBLISH":
                    if (parts.Length >= 3)
                    {
                        var count = _pubSubManager.Publish(parts[1], parts[2]);
                        response = $"Delivered to {count} subscriber(s)";
                    }
                    break;
            }

            await writer.WriteLineAsync(response);
        }

        client.Close();
    }
}