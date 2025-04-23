using Rudy.Server;
using Rudy.Server.Builders;
using Rudy.Server.Stores;

namespace Rudy.Tests;

public class MemoryStoreTests
{
    private readonly MemoryStore _memoryStore = MemoryStore.Create();

    [Fact]
    public void SetAndGet_ShouldReturnCorrectValue()
    {
        _memoryStore.Set("foo", "bar");
        var value = _memoryStore.Get("foo");

        Assert.Equal("bar", value);
    }

    [Fact]
    public void Delete_ShouldRemoveKey()
    { 
        _memoryStore.Set("key", "value");
        var deleted = _memoryStore.Delete("key");
        var result = _memoryStore.Get("key");

        Assert.True(deleted);
        Assert.Null(result);
    }

    [Fact]
    public async Task ExpiredKey_ShouldBeNull()
    {
        _memoryStore.Set("temp", "123", TimeSpan.FromMilliseconds(100));
        await Task.Delay(200);

        var result = _memoryStore.Get("temp");
        Assert.Null(result);
    }

    [Fact]
    public void OverwriteKey_ShouldReplaceValue()
    {
        _memoryStore.Set("counter", 1);
        _memoryStore.Set("counter", 2);

        var result = _memoryStore.Get("counter");
        Assert.Equal(2, result);
    }
}