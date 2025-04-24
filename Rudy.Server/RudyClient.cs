using System.Net.Sockets;

namespace Rudy.Server;

public class RudyClient : IDisposable
{
    private readonly TcpClient _client;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    public RudyClient(string host, int port)
    {
        _client = new TcpClient();
        _client.Connect(host, port);
        var stream = _client.GetStream();
        _reader = new StreamReader(stream);
        _writer = new StreamWriter(stream) { AutoFlush = true };
    }

    public async Task SendAsync(string line)
    {
        await _writer.WriteLineAsync(line);
    }

    public async Task<string?> ReceiveAsync()
    {
        return await _reader.ReadLineAsync();
    }

    public void Dispose()
    {
        _client.Dispose();
        _reader.Dispose();
        _writer.Dispose();
    }
}