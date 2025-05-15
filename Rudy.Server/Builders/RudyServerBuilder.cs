using System.Net;
using Rudy.Server.Managers;
using Rudy.Server.Processors;
using Rudy.Server.Stores;

namespace Rudy.Server.Builders;

public class RudyServerBuilder
{
    private readonly MemoryStore _memoryStore;
    private int _port;
    private IPAddress _ipAddress;
    private DiskStore? _diskStore;
    private string? _logPath;

    private RudyServerBuilder()
    {
        _ipAddress = IPAddress.Any;
        _memoryStore = MemoryStore.Create();
    }

    public static RudyServerBuilder Initialize()
    {
        return new RudyServerBuilder();
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

    public RudyServerBuilder WithDiskStore(string path)
    {
        _diskStore = new DiskStore(Path.Combine(path, ".db"));
        return this;
    }

    public RudyServerBuilder WithLogging(string? logPath = null)
    {
        _logPath = logPath;
        return this;
    }

    public RudyServer Build()
    {
        Logger.Initialize(_logPath);
        var replicaManager = new ReplicaManager(new CommandProcessor(_memoryStore,  _diskStore));
        var tcpClientManager = new TcpClientManager(replicaManager, new PubSubManager(), _memoryStore, _diskStore);
        return new RudyServer(_ipAddress, _port, replicaManager, tcpClientManager);
    }
}