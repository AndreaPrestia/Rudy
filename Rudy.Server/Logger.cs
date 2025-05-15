namespace Rudy.Server;

internal static class Logger
{
    private static string? _logFilePath;
    private static bool _isInitialized;

    public static void Initialize(string? logFilePath = null)
    {
        if (_isInitialized) return;

        var logFileName = $"rudy_{DateTime.Now:yyyyMMdd}.log";

        _logFilePath =
            Path.Combine(string.IsNullOrWhiteSpace(logFilePath) ? AppDomain.CurrentDomain.BaseDirectory : logFilePath,
                logFileName);

        var dir = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        _isInitialized = true;
    }

    public static void Info(string message)
    {
        Log($"INFO ℹ️ {message}");
    }

    public static void Warning(string message)
    {
        Log($"WARNING ⚠️ {message}");
    }

    public static void Error(string message)
    {
        Log($"ERROR ❌ {message}");
    }

    private static void Log(string message)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        var timestamped = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        File.AppendAllText(_logFilePath ?? throw new InvalidOperationException(),
            timestamped + Environment.NewLine);
    }
}