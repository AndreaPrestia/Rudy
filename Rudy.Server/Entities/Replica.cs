using System.Net.Sockets;

namespace Rudy.Server.Entities;

public class Replica
{
    public Guid Id { get; set; }
    public string? IpAddress { get; set; }
    public TcpClient? TcpClient { get; set; }
}