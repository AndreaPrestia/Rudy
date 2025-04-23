using Rudy.Server.Managers;
using Rudy.Server.Stores;

namespace Rudy.Server.Builders;

public class RudyServerBuilder
{
    private readonly DiskStore _diskStore;
    private readonly PubSubManager _pubSubManager;
    private readonly ReplicaManager _replicaManager;
    private readonly MemoryStore _memoryStore;
    private int _port;

    private RudyServerBuilder()
    {
        _memoryStore = MemoryStore.Create();
        _diskStore = new DiskStore();
        _pubSubManager = new PubSubManager();
        _replicaManager = new ReplicaManager();
    }

    public static RudyServerBuilder Initialize()
    {
        return new RudyServerBuilder();
    }

    public RudyServerBuilder WithPort(int port)
    {
        _port = port;
        return this;
    }

    public TcpServer Build()
    {
        return new TcpServer(_port, _replicaManager, _pubSubManager, _diskStore, _memoryStore);
    }
    
}