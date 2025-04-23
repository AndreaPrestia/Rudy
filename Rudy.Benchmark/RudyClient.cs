using System.Net.Sockets;

namespace Rudy.Benchmark;

public class RudyClient
{
    private readonly StreamWriter _writer;
    private readonly StreamReader _reader;

    public RudyClient(string host, int port)
    {
        var client = new TcpClient();
        client.Connect(host, port);
        var stream = client.GetStream();
        _writer = new StreamWriter(stream) { AutoFlush = true };
        _reader = new StreamReader(stream);
    }

    public async Task SendAsync(string cmd)
    {
        await _writer.WriteLineAsync(cmd);
    }

    public async Task<string?> ReceiveAsync()
    {
        return await _reader.ReadLineAsync();
    }
}