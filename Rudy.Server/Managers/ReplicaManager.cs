using System.Net.Sockets;

namespace Rudy.Server.Managers;

internal class ReplicaManager
{
    private static readonly List<TcpClient> Replicas = new();

    public void RegisterReplica(TcpClient replica)
    {
        lock (Replicas)
        {
            Replicas.Add(replica);
        }
    }

    public void Broadcast(string commandLine)
    {
        lock (Replicas)
        {
            foreach (var replica in Replicas.ToList())
            {
                try
                {
                    var writer = new StreamWriter(replica.GetStream()) { AutoFlush = true };
                    writer.WriteLine(commandLine);
                }
                catch
                {
                    Replicas.Remove(replica);
                }
            }
        }
    }
}
