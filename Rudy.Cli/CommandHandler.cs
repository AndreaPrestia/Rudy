using Rudy.Cli.Helpers;

namespace Rudy.Cli;

public static class CommandHandler
{
    public static async Task Execute(RudyClient client, string input)
    {
        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) return;

        var command = tokens[0].ToUpper();

        if (command == "SUBSCRIBE" && tokens.Length >= 2)
        {
            await client.SendAsync(input);
            ColorConsole.WriteInfo($"Subscribed to {tokens[1]}");

            while (true)
            {
                var msg = await client.ReceiveAsync();
                if (msg == null) break;
                ColorConsole.Write("→ " + msg, ConsoleColor.Cyan);
            }
        }
        else
        {
            await client.SendAsync(input);
            var response = await client.ReceiveAsync();
            ColorConsole.Write(response ?? "(nil)", ConsoleColor.Green);
        }
    }
}
