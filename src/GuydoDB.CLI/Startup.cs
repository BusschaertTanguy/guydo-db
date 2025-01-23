using System.Text;
using Microsoft.Extensions.Options;

namespace GuydoDB.CLI;

internal sealed class Startup(IOptions<AppConfiguration> configuration)
{
    public async Task StartAsync()
    {
        var quit = false;

        while (!quit)
        {
            Console.Write("[GUYDO] > ");
            var entry = Console.ReadLine();

            if (!TryParseCommand(entry, out var command))
            {
                Console.WriteLine($"[GUYDO] > Invalid command: {entry}");
                continue;
            }

            switch (command)
            {
                case QuitCommand:
                    quit = true;
                    continue;
                case CreateDatabaseCommand createDatabaseCommand:
                    HandleCreateDatabase(createDatabaseCommand);
                    continue;
                case CreateTableCommand createTableCommand:
                    await HandleCreateTable(createTableCommand);
                    continue;
                case AddCommand addCommand:
                    await HandleAdd(addCommand);
                    continue;
                case GetCommand getCommand:
                    await HandleGet(getCommand);
                    continue;
                default:
                    continue;
            }
        }
    }

    private void HandleCreateDatabase(CreateDatabaseCommand command)
    {
        var databasePath = Path.Combine(configuration.Value.Root, "db", command.Name);

        if (Directory.Exists(databasePath))
        {
            Console.WriteLine($"[GUYDO] > Database: {command.Name} already exists");
            return;
        }

        Directory.CreateDirectory(databasePath);
        Console.WriteLine($"[GUYDO] > Database {command.Name} created");
    }

    private async Task HandleCreateTable(CreateTableCommand command)
    {
        var databasePath = Path.Combine(configuration.Value.Root, "db", command.Database);
        if (!Directory.Exists(databasePath))
        {
            Console.WriteLine($"[GUYDO] > Database: {command.Database} does not exist");
            return;
        }

        var tablePath = Path.Combine(databasePath, command.Name);
        if (File.Exists(tablePath))
        {
            Console.WriteLine($"[GUYDO] > Table: {command.Name} already exists");
            return;
        }

        var page = Page.Create();
        await File.WriteAllBytesAsync(tablePath, page.Data);
        Console.WriteLine($"[GUYDO] > Table {command.Database}.{command.Name} created");
    }

    private async Task HandleAdd(AddCommand command)
    {
        var databasePath = Path.Combine(configuration.Value.Root, "db", command.Database);
        if (!Directory.Exists(databasePath))
        {
            Console.WriteLine($"[GUYDO] > Database: {command.Database} does not exist");
            return;
        }

        var tablePath = Path.Combine(databasePath, command.Table);
        if (!File.Exists(tablePath))
        {
            Console.WriteLine($"[GUYDO] > Table: {command.Table} does not exist");
            return;
        }

        var bytes = await File.ReadAllBytesAsync(tablePath);
        var page = new Page(bytes);

        page.Add(Encoding.UTF8.GetBytes(command.Data));
        await File.WriteAllBytesAsync(tablePath, page.Data);
        Console.WriteLine("[GUYDO] > Record added");
    }

    private async Task HandleGet(GetCommand command)
    {
        var databasePath = Path.Combine(configuration.Value.Root, "db", command.Database);
        if (!Directory.Exists(databasePath))
        {
            Console.WriteLine($"[GUYDO] > Database: {command.Database} does not exist");
            return;
        }

        var tablePath = Path.Combine(databasePath, command.Table);
        if (!File.Exists(tablePath))
        {
            Console.WriteLine($"[GUYDO] > Table: {command.Table} does not exist");
            return;
        }

        var page = new Page(await File.ReadAllBytesAsync(tablePath));
        page.ForEach(bytes => Console.WriteLine(Encoding.UTF8.GetString(bytes)));
    }

    private static bool TryParseCommand(string? command, out object? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(command))
        {
            return false;
        }

        var arguments = command.Split(" ");
        if (arguments.Length < 1)
        {
            return false;
        }

        result = arguments[0] switch
        {
            "Q" => new QuitCommand(),
            "QUIT" => new QuitCommand(),
            "CREATE" => arguments[1] switch
            {
                "DATABASE" => new CreateDatabaseCommand(arguments[2]),
                "TABLE" => new CreateTableCommand(arguments[2].Split(".")[0], arguments[2].Split(".")[1]),
                _ => null
            },
            "ADD" => new AddCommand(arguments[1].Split(".")[0], arguments[1].Split(".")[1], string.Join(" ", arguments[2..])),
            "GET" => new GetCommand(arguments[1].Split(".")[0], arguments[1].Split(".")[1]),
            _ => null
        };

        return result is not null;
    }

    private sealed record QuitCommand;

    private sealed record CreateDatabaseCommand(string Name);

    private sealed record CreateTableCommand(string Database, string Name);

    private sealed record AddCommand(string Database, string Table, string Data);

    private sealed record GetCommand(string Database, string Table);
}