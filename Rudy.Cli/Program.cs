using System.CommandLine;
using Rudy.Cli;

var root = new RootCommand("Rudy CLI");

var commandArg = new Argument<string>("command", "The command to execute");
var hostOpt = new Option<string>("--host", () => "127.0.0.1", "Server host");
var portOpt = new Option<int>("--port", () => 6379, "Server port");

var repl = new Command("repl", "Start interactive REPL mode");
repl.SetHandler(async (host, port) =>
{
    var redis = new RudyClient(host, port);
    var replEngine = new ReplEngine(redis);
    await replEngine.RunAsync();
}, hostOpt, portOpt);

// Direct command
var exec = new Command("exec", "Send command directly")
{
    commandArg,
    hostOpt,
    portOpt
};

exec.SetHandler(async (command, host, port) =>
{
    var client = new RudyClient(host, port);
    await CommandHandler.Execute(client, command);
}, commandArg, hostOpt, portOpt);

root.AddOption(hostOpt);
root.AddOption(portOpt);
root.AddCommand(repl);
root.AddCommand(exec);

await root.InvokeAsync(args);