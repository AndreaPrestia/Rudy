namespace Rudy.Server.Stores;

internal class DiskStore(string filename)
{
    private readonly Lock _lock = new();

    public void Log(string commandLine)
    {
        lock (_lock)
        {
            File.AppendAllText(filename, commandLine + "\n");
        }
    }

    public IEnumerable<string> Replay()
    {
        if (!File.Exists(filename)) yield break;
        foreach (var line in File.ReadAllLines(filename))
        {
            yield return line;
        }
    }
}