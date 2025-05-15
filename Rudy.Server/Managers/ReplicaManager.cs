using System.Net.Sockets;
using Rudy.Server.Entities;
using Rudy.Server.Processors;

namespace Rudy.Server.Managers;

internal class ReplicaManager(CommandProcessor commandProcessor)
{
    private readonly List<Replica> _replicas = [];

    public void RegisterReplica(TcpClient tcpClient)
    {
        lock (_replicas)
        {
            var replica = new Replica
            {
                Id = Guid.NewGuid(),
                IpAddress = tcpClient.Client.RemoteEndPoint?.ToString(),
                TcpClient = tcpClient
            };
            
            _replicas.Add(replica);

            Logger.Info($"[Replica] registered {replica.Id} - {replica.IpAddress}");
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
                    if (replica.TcpClient == null)
                    {
                        Logger.Warning($"[Replica] {replica.Id} - {replica.IpAddress} has no active tcp client");
                        continue;
                    }
                    
                    var writer = new StreamWriter(replica.TcpClient.GetStream()) { AutoFlush = true };
                    writer.WriteLine(commandLine);
                    Logger.Info($"[Replica] written on {replica.Id} - {replica.IpAddress}: {commandLine}");
                }
                catch(Exception e)
                {
                    _replicas.Remove(replica);
                    Logger.Warning($"[Replica] failure {replica.Id} - {replica.IpAddress}: {e.Message}");
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

            await writer.WriteLineAsync("CLONE");

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var command = await reader.ReadLineAsync();
                    if (command == null) break;

                    try
                    {
                       var replicatedCommandApplied = commandProcessor.ApplyReplicatedCommand(command);
                       Logger.Info($"$[Replica] Applied command: {command} - {replicatedCommandApplied}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"[Replica] Failed to apply command: {command} - {ex.Message}");
                    }
                }
            });

            Logger.Info($"[Replica] Connected to master at {host}:{port}");
        }
        catch (Exception ex)
        {
            Logger.Warning($"[Replica] Failed to connect to master: {ex.Message}");
        }
    }
}
