using System.Net.Sockets;
using Rudy.Server.Stores;

namespace Rudy.Server.Managers;

internal class TcpClientManager(
    ReplicaManager replicaManager,
    PubSubManager pubSubManager,
    MemoryStore memoryStore,
    DiskStore diskStore)
{
    public async Task HandleClientAsync(TcpClient client)
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

            diskStore.Log(line);

            switch (isReplica)
            {
                case false when line.Equals("CLONE", StringComparison.CurrentCultureIgnoreCase):
                    isReplica = true;
                    replicaManager.RegisterReplica(client);
                    continue;
                case true:
                    continue;
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

                        memoryStore.Set(key, val, ttl);
                        replicaManager.Broadcast(line);
                        response = "OK";
                    }

                    break;

                case "GET":
                    if (parts.Length >= 2)
                    {
                        var val = memoryStore.Get(parts[1]);
                        response = val?.ToString() ?? "(nil)";
                    }

                    break;

                case "DEL":
                    if (parts.Length >= 2)
                    {
                        var removed = memoryStore.Delete(parts[1]);
                        replicaManager.Broadcast(line);
                        response = removed ? "1" : "0";
                    }

                    break;

                case "SUB":
                    if (parts.Length >= 2)
                    {
                        pubSubManager.Subscribe(parts[1], client);
                        response = $"Subscribed to {parts[1]}";
                    }

                    break;

                case "PUB":
                    if (parts.Length >= 3)
                    {
                        var count = pubSubManager.Publish(parts[1], parts[2]);
                        response = $"Delivered to {count} subscriber(s)";
                    }

                    break;

                case "HEALTH":
                    response = "OK";
                    break;
            }

            await writer.WriteLineAsync(response);
        }

        client.Close();
    }
}