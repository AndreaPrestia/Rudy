using Rudy.Server.Stores;

namespace Rudy.Server.Processors;

internal class CommandProcessor(MemoryStore store, DiskStore? disk)
{
    public bool ApplyReplicatedCommand(string command)
    {
        var parts = command.Split(' ', 3);
        var cmd = parts[0].ToUpperInvariant();

        disk?.Log(command);

        switch (cmd)
        {
            case "SET":
                store.Set(parts[1], parts[2]);
                return true;
            case "DEL":
                return store.Delete(parts[1]);
            default:
                return false;
        }
    }
}