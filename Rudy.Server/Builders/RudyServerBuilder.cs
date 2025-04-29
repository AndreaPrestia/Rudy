using System.Net;
using Rudy.Server.Managers;
using Rudy.Server.Processors;
using Rudy.Server.Stores;

namespace Rudy.Server.Builders;

public class RudyServerBuilder
{
    private readonly DiskStore _diskStore;
    private readonly PubSubManager _pubSubManager;
    private readonly ReplicaManager _replicaManager;
    private readonly MemoryStore _memoryStore;
    private readonly TcpClientManager _tcpClientManager;
    private int _port;
    private IPAddress _ipAddress;

    private RudyServerBuilder(string fileName)
    {
        _ipAddress = IPAddress.Any;
        _memoryStore = MemoryStore.Create();
        _diskStore = new DiskStore(fileName);
        _pubSubManager = new PubSubManager();
        _replicaManager = new ReplicaManager(new CommandProcessor(_memoryStore, _diskStore));
        _tcpClientManager = new TcpClientManager(_replicaManager, _pubSubManager, _memoryStore, _diskStore);
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
    
    public RudyServerBuilder WithIpAddress(IPAddress ipAddress)
    {
        _ipAddress = ipAddress;
        return this;
    }

    public RudyServerBuilder WithPort(int port)
    {
        _port = port;
        return this;
    }

    public RudyServer Build()
    {
        return new RudyServer(_ipAddress, _port, _replicaManager, _pubSubManager, _diskStore, _memoryStore, _tcpClientManager);
    }
}