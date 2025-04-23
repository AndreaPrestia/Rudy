using System.Collections.Concurrent;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Rudy.Server.Stores;

internal class MemoryStore
{
    private readonly ConcurrentDictionary<string, (object value, DateTime? expiresAt)> _store = new();
    private readonly Timer _timer;

    private MemoryStore()
    {
        _timer = new Timer();
        _timer.Interval = 2000;
        _timer.Elapsed += OnTimedEvent!;
        _timer.AutoReset = true;
        _timer.Enabled = true;
    }
    
    internal static MemoryStore Create()
    {
        return new MemoryStore();
    }

    public void Set(string key, object value, TimeSpan? ttl = null)
    {
        DateTime? expiration = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : null;
        _store[key] = (value, expiration);
    }

    public object? Get(string key)
    {
        if (!_store.TryGetValue(key, out var entry)) return null;
        
        if (entry.expiresAt == null || entry.expiresAt > DateTime.UtcNow)
        {
            return entry.value;
        }

        _store.TryRemove(key, out _); // expired
        return null;
    }

    public bool Delete(string key) => _store.TryRemove(key, out _);
    
    private void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        var now = DateTime.UtcNow;
        foreach (var key in _store.Keys)
        {
            if (_store.TryGetValue(key, out var entry) && entry.expiresAt < now)
            {
                _store.TryRemove(key, out _);
            }
        }
    }
}