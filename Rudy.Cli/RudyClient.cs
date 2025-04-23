using System.Net.Sockets;

namespace Rudy.Cli;

public class RudyClient
{
    private readonly string _host;
    private readonly int _port;
    private TcpClient _client = null!;
    private StreamWriter _writer = null!;
    private StreamReader _reader = null!;

    public RudyClient(string host, int port)
    {
        _host = host;
        _port = port;
        ConnectWithRetry().Wait();
    }

    private async Task ConnectWithRetry()
    {
        for (var attempts = 1; attempts <= 5; attempts++)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_host, _port);
                var stream = _client.GetStream();
                _writer = new StreamWriter(stream) { AutoFlush = true };
                _reader = new StreamReader(stream);
                return;
            }
            catch
            {
                Console.WriteLine($"[!] Connection attempt {attempts} failed...");
                await Task.Delay(1000);
            }
        }

        throw new Exception("Failed to connect to server.");
    }

    public async Task SendAsync(string command)
    {
        await _writer.WriteLineAsync(command);
    }

    public async Task<string?> ReceiveAsync()
    {
        return await _reader.ReadLineAsync();
    }

    public StreamReader Reader => _reader;
}
