namespace Rudy.Server.Stores;

public class DiskStore
{
    private const string File = "aof.log";
    private readonly Lock Lock = new();

    public void Log(string commandLine)
    {
        lock (Lock)
        {
            System.IO.File.AppendAllText(File, commandLine + "\n");
        }
    }

    public IEnumerable<string> Replay()
    {
        if (!System.IO.File.Exists(File)) yield break;
        foreach (var line in System.IO.File.ReadAllLines(File))
        {
            yield return line;
        }
    }
}