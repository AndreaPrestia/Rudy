using System.Collections.Concurrent;
using System.Net.Sockets;
using Rudy.Server.Entities;

namespace Rudy.Server.Managers;

internal class PubSubManager
{
    private static readonly ConcurrentDictionary<string, List<Subscription>> Subscriptions = new();

    public void Subscribe(string channel, TcpClient client)
    {
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            IpAddress = client.Client.RemoteEndPoint?.ToString(),
            TcpClient = client
        };
        
        Subscriptions.AddOrUpdate(channel,
            _ => [subscription],
            (_, list) => {
                lock (list) list.Add(subscription);
                return list;
            });
        
        Logger.Info($"[Subscription] registered {subscription.Id} - {subscription.IpAddress} on channel {channel}");
    }

    public int Publish(string channel, string message)
    {
        if (!Subscriptions.TryGetValue(channel, out var subscriptions)) return 0;

        var result = 0;
        lock (subscriptions)
        {
            foreach (var subscription in subscriptions.ToList())
            {
                try
                {
                    if (subscription.TcpClient == null)
                    {
                        Logger.Warning($"[Subscription] {subscription.Id} - {subscription.IpAddress} has no active tcp client");
                        continue;
                    }
                    
                    var writer = new StreamWriter(subscription.TcpClient.GetStream()) { AutoFlush = true };
                    writer.WriteLine($"message {channel} {message}");
                    Logger.Info($"[Publish] written on {channel} - {message} for subscription {subscription.Id} - {subscription.IpAddress}");
                    Interlocked.Add(ref result, 1);
                }
                catch(Exception e)
                {
                    subscriptions.Remove(subscription);
                    Logger.Warning($"[Publish] failure {subscription.Id} - {subscription.IpAddress}: {e.Message}");
                }
            }
            return result;
        }
    }
}