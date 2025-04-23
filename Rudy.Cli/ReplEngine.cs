using Rudy.Cli.Helpers;

namespace Rudy.Cli;

public class ReplEngine(RudyClient client)
{
    private readonly List<string> _history = new();

    public async Task RunAsync()
    {
        ColorConsole.WriteInfo("Connected! Type 'exit' to quit.");

        while (true)
        {
            Console.Write("redis> ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;
            if (input.Trim().Equals("exit", StringComparison.CurrentCultureIgnoreCase)) break;

            _history.Add(input);
            await CommandHandler.Execute(client, input);
        }
    }
}