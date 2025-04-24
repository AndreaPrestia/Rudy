using System.Net.Sockets;
using Rudy.Server.Processors;

namespace Rudy.Server.Managers;

internal class ReplicaManager(CommandProcessor commandProcessor)
{
    private readonly List<TcpClient> _replicas = [];

    public void RegisterReplica(TcpClient replica)
    {
        lock (_replicas)
        {
            _replicas.Add(replica);
        }
    }

    public void Broadcast(string commandLine)
    {
        lock (_replicas)
        {
            foreach (var replica in _replicas.ToList())
            {
                try
                {
                    var writer = new StreamWriter(replica.GetStream()) { AutoFlush = true };
                    writer.WriteLine(commandLine);
                }
                catch
                {
                    _replicas.Remove(replica);
                }
            }
        }
    }
    
    public async Task ConnectToMaster(string host, int port)
    {
        try
        {
            var client = new TcpClient();
            await client.ConnectAsync(host, port);
            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream) { AutoFlush = true };

            // Register with master
            await writer.WriteLineAsync("REPLICA_REGISTER");

            // Start listening for replication commands
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var command = await reader.ReadLineAsync();
                    if (command == null) break; // disconnected

                    try
                    {
                       var replicatedCommandApplied = commandProcessor.ApplyReplicatedCommand(command);
                       Console.WriteLine($"$[Replica] Applied command: {command} - {replicatedCommandApplied}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Replica] Failed to apply command: {command} - {ex.Message}");
                    }
                }
            });

            Console.WriteLine($"[Replica] Connected to master at {host}:{port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Replica] Failed to connect to master: {ex.Message}");
        }
    }
}
