using System.Net;
using Rudy.Server.Managers;
using Rudy.Server.Processors;
using Rudy.Server.Stores;

namespace Rudy.Server.Builders;

public class RudyServerBuilder
{
    private readonly ReplicaManager _replicaManager;
    private readonly TcpClientManager _tcpClientManager;
    private int _port;
    private IPAddress _ipAddress;

    private RudyServerBuilder(string fileName)
    {
        _ipAddress = IPAddress.Any;
        var memoryStore = MemoryStore.Create();
        var diskStore = new DiskStore(fileName);
        _replicaManager = new ReplicaManager(new CommandProcessor(memoryStore, diskStore));
        _tcpClientManager = new TcpClientManager(_replicaManager, new PubSubManager(), memoryStore, diskStore);
    }

    public static RudyServerBuilder Initialize(string fileName)
    {
        return new RudyServerBuilder(fileName);
    }

    public RudyServerBuilder WithIpAddress(string ipAddress)
    {
        _ipAddress = IPAddress.Parse(ipAddress);
        return this;
    }

    public RudyServerBuilder WithPort(int port)
    {
        _port = port;
        return this;
    }

    public RudyServer Build()
    {
        return new RudyServer(_ipAddress, _port, _replicaManager, _tcpClientManager);
    }
}