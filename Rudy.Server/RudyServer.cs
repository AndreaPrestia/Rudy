using System.Net;
using System.Net.Sockets;
using Rudy.Server.Managers;

namespace Rudy.Server;

public class RudyServer
{
    private readonly IPAddress _ipAddress;
    private readonly int _port;
    private readonly CancellationTokenSource _cts = new();
    private readonly TcpListener _listener;
    private readonly ReplicaManager _replicaManager;
    private readonly TcpClientManager _tcpClientManager;
    
    internal RudyServer(IPAddress ipAddress, int port, ReplicaManager replicaManager, TcpClientManager tcpClientManager)
    {
        _ipAddress = ipAddress;
        _port = port;
        _replicaManager = replicaManager;
        _tcpClientManager = tcpClientManager;
        _listener = new TcpListener(_ipAddress, _port);
    }

    public string Host =>  _ipAddress.MapToIPv4().ToString();
    public int Port => _port;
    
    public void Start()
    {
        _listener.Start();
        _ = Task.Run(() => AcceptLoop(_cts.Token));
    }
    
    public void Stop()
    {
        _cts.Cancel();
        _listener.Stop();
    }
    
    public Task ConnectAsReplicaAsync(string masterHost, int masterPort)
    {
        return _replicaManager.ConnectToMaster(masterHost, masterPort);
    }

    private async Task AcceptLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync(token);
            }
            catch (OperationCanceledException) { break; }

            _ = Task.Run(() => _tcpClientManager.HandleClientAsync(client), _cts.Token);
        }
    }
}