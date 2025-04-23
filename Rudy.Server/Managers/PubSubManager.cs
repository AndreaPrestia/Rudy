using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Rudy.Server.Managers;

internal class PubSubManager
{
    private static readonly ConcurrentDictionary<string, List<TcpClient>> Subscriptions = new();

    public void Subscribe(string channel, TcpClient client)
    {
        Subscriptions.AddOrUpdate(channel,
            _ => [client],
            (_, list) => {
                lock (list) list.Add(client);
                return list;
            });
    }

    public int Publish(string channel, string message)
    {
        if (!Subscriptions.TryGetValue(channel, out var clients)) return 0;
        lock (clients)
        {
            foreach (var client in clients.ToList())
            {
                try
                {
                    var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                    writer.WriteLine($"message {channel} {message}");
                }
                catch
                {
                    // Clean dead clients
                    clients.Remove(client);
                }
            }
            return clients.Count;
        }
    }
}