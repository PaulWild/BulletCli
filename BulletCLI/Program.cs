using System.CommandLine;
using BulletCLI;
using BulletCLI.Model;
using BulletCLI.Todos;
using Dapper;

SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

var date = DateOnly.FromDateTime(DateTime.Today);


var rootCommand = new RootCommand("Bullet CLI");
var cancellationSource = new CancellationTokenSource();



rootCommand.SetHandler(async () =>
{
    var entries = new Entries(date, cancellationSource.Token);
    var printer = new ConsolePrinter();
    entries.Subscribe(printer);
    
    while (true)
    {
        var key = Console.ReadKey(true);
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                await entries.SelectPrevious();
                break;
            case ConsoleKey.DownArrow:
                await entries.SelectNext();
                break;
            case ConsoleKey.LeftArrow:
                date = date.AddDays(-1);
                await entries.ChangeDate(date);
                break;
            case ConsoleKey.RightArrow:
                date = date.AddDays(1);
                await entries.ChangeDate(date);
                break;
            case ConsoleKey.Q:
                Console.Clear();
                return;
            case ConsoleKey.D:
                await entries.UpdateEntry(EntryType.TodoDone);
                break;
            case ConsoleKey.T:
                await entries.UpdateEntry(EntryType.Todo);
                break;
            case ConsoleKey.M:
                await entries.UpdateEntry(EntryType.TodoMigrated);
                break;
            case ConsoleKey.Spacebar:
                printer.EditMode((entryType, message) => entries.AddEntry(entryType, message));
                break;
        }
    }
});

await rootCommand.InvokeAsync(args);


