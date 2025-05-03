using RconBroadcast;
using RconSharp;

var a = IntervalParser.ParseInterval("20m");

var config = await ConfigManager.LoadConfigAsync();
if(config.Servers.Length == 0) {
    ServerConfig defaultServer = new() {
        Commands = [new()]
    };

    config.Servers = [defaultServer];
    await ConfigManager.SaveConfigAsync(config);

    Console.WriteLine("Created default config, exiting process");
    Environment.Exit(0);
    return;
}

List<Thread> threads = [];
foreach(var serverConfig in config.Servers) {
    Thread thread = new(() => LoopServerAsync(serverConfig, config.ParsedInterval).GetAwaiter().GetResult());
    thread.Start();
    threads.Add(thread);
}

foreach(var thread in threads) {
    thread.Join();
}

Console.WriteLine("Completed all threads, exiting");

static async Task LoopServerAsync(ServerConfig serverConfig, TimeSpan reconnectInterval) {
    var client = RconClient.Create(serverConfig.ServerIp, serverConfig.ServerPort);

TRY_CONNECT:
    try {
        await client.ConnectAsync();

        var authenticated = await client.AuthenticateAsync(serverConfig.Password);
        if(authenticated) {

            List<Func<Task>> commandTasks = [];
            foreach(var command in serverConfig.Commands) {
                commandTasks.Add(() => LoopCommandAsync(client, serverConfig, command));
            }

            Console.WriteLine($"{serverConfig.ServerIp}:{serverConfig.ServerPort} Registered {commandTasks.Count} command(s)");

            await Task.WhenAll(commandTasks.Select(x => x()));
            Console.WriteLine($"{serverConfig.ServerIp}:{serverConfig.ServerPort} Completed all tasks");

        } else {
            Console.WriteLine($"Failed to authenticate with {serverConfig.ServerIp}:{serverConfig.ServerPort}");
            await Task.Delay(reconnectInterval);
            goto TRY_CONNECT;
        }
    } catch(Exception ex) {
        Console.WriteLine($"Failed to connect to {serverConfig.ServerIp}:{serverConfig.ServerPort} {ex.Message}");
        await Task.Delay(reconnectInterval);
        goto TRY_CONNECT;
    }
}

static async Task LoopCommandAsync(RconClient client, ServerConfig serverConfig, CommandConfig commandConfig) {
    while(true) {
        try {
            Console.WriteLine($"{serverConfig.ServerIp}:{serverConfig.ServerPort} EXEC: {commandConfig.Command}");

            string response = await client.ExecuteCommandAsync(commandConfig.Command);
            if(!string.IsNullOrWhiteSpace(response)) {
                Console.WriteLine($"{serverConfig.ServerIp}:{serverConfig.ServerPort} RESP: {response}");
            }

            if(commandConfig.ParsedInterval == TimeSpan.Zero) {
                break;
            }

            await Task.Delay(commandConfig.ParsedInterval);
        } catch(Exception ex) {
            Console.WriteLine($"{serverConfig.ServerIp}:{serverConfig.ServerPort} ERR: {ex.Message}");
            if(commandConfig.ParsedInterval == TimeSpan.Zero) {
                break;
            }

            await Task.Delay(commandConfig.ParsedInterval);
        }
    }
}