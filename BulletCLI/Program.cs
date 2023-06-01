using System.CommandLine;
using System.Runtime.InteropServices;
using System.Text;
using BulletCLI;
using BulletCLI.Model;
using BulletCLI.Todos;


Console.OutputEncoding = Encoding.UTF8;

var db = new TodoContext();
//await db.Database.MigrateAsync();
var date = DateOnly.FromDateTime(DateTime.Today);


string FormatEntry(EntryType type)
{
    return type switch
    {
        EntryType.Event => "\u2605",
        EntryType.Note => "\u266A",
        EntryType.Todo => "\u22C5",
        EntryType.TodoDone => "\u2613",
        EntryType.TodoMigrated => "\u27A4",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}

void Draw(int origRow, int origCol, IList<TodoDto> entries)
{
    Console.CursorVisible = false;
    Console.Clear();
    Console.WriteLine($"{date:D}");
    for (var i = 0; i < Console.WindowWidth; i++)
    {
        Console.Write('-');
    }
    
    Console.WriteLine();

    foreach (var entry in entries)
    {
        Console.WriteLine($"{FormatEntry(entry.EntryType)} {entry.Message}");
    }

    Console.SetCursorPosition(origRow, origCol + 1);
}

bool IsTodo(EntryType type)
{
    return type switch
    {
        EntryType.Todo => true,
        EntryType.TodoDone => true,
        EntryType.TodoMigrated => true,
        _ => false
    };
}

var rootCommand = new RootCommand("Bullet CLI");
var cancellationSource = new CancellationTokenSource();

rootCommand.SetHandler(async () =>
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        Console.SetBufferSize(1000, 1000);
    }

    var entries = await new GetHandler(db).Handle(new Get(date), cancellationSource.Token);
    var origRow = Console.CursorLeft;
    var origCol = Console.CursorTop;
    Draw(origRow, origCol, entries);
    var currentRow = 1;
    var index = -1;

    var exit = false;
    while (!exit)
    {
        var key = Console.ReadKey(true);
        var windowWidth = Console.WindowWidth;

        switch (key.Key)
        {
            case (ConsoleKey.UpArrow):
                if (index <= 0)
                {
                    break;
                }

                Console.SetCursorPosition(0, origRow + currentRow);
                Output.Write($"{FormatEntry(entries[index].EntryType)} {entries[index].Message}")
                    .WithBackgroundColour(ConsoleColor.Black).Run();


                currentRow -= 1;
                index -= 1;
                Console.SetCursorPosition(0, origRow + currentRow);
                Output.Write($"{FormatEntry(entries[index].EntryType)} {entries[index].Message}")
                    .WithBackgroundColour(ConsoleColor.Gray).WithForegroundColour(ConsoleColor.Black).Run();

                break;
            case (ConsoleKey.DownArrow):
                if (index >= entries.Count - 1)
                {
                    break;
                }

                if (currentRow >= 2)
                {
                    Console.SetCursorPosition(0, origRow + currentRow);
                    Output.Write($"{FormatEntry(entries[index].EntryType)} {entries[index].Message}")
                        .WithBackgroundColour(ConsoleColor.Black).Run();
                }

                currentRow += 1;
                index += 1;
                Console.SetCursorPosition(0, origRow + currentRow);
                Output.Write($"{FormatEntry(entries[index].EntryType)} {entries[index].Message}")
                    .WithBackgroundColour(ConsoleColor.Gray).WithForegroundColour(ConsoleColor.Black).Run();

                break;
            case (ConsoleKey.LeftArrow):
                date = date.AddDays(-1);
                entries =  await new GetHandler(db).Handle(new Get(date), cancellationSource.Token);
                Draw(origRow, origCol, entries);
                currentRow = 1;
                index = -1;
                
                break;
            case (ConsoleKey.RightArrow):
                date = date.AddDays(1);
                entries =  await new GetHandler(db).Handle(new Get(date), cancellationSource.Token);

                Draw(origRow, origCol, entries);
                currentRow = 1;
                index = -1;
                
                break;
            case (ConsoleKey.Q):
                exit = true;
                Console.Clear();
                break;
            case (ConsoleKey.D):
                if (index >= 0)
                {
                    if (IsTodo(entries[index].EntryType))
                    {
                        await new UpdateHandler(db).Handle(new Update(entries[index].Id, entries[index].Date,
                            EntryType.TodoDone), cancellationSource.Token);
                        
                         entries = await new GetHandler(db).Handle(new Get(date), cancellationSource.Token);

                    }

                    Console.SetCursorPosition(0, origRow + currentRow);
                    Output.Write($"{FormatEntry(entries[index].EntryType)} {entries[index].Message}")
                        .WithBackgroundColour(ConsoleColor.Gray).WithForegroundColour(ConsoleColor.Black).Run();

                }

                break;
            case (ConsoleKey.T):
                if (index >= 0)
                {
                    if (IsTodo(entries[index].EntryType))
                    {
                        await new UpdateHandler(db).Handle(new Update(entries[index].Id, entries[index].Date,
                            EntryType.Todo), cancellationSource.Token);
                        
                        entries =  await new GetHandler(db).Handle(new Get(date), cancellationSource.Token);
                    }

                    Console.SetCursorPosition(0, origRow + currentRow);
                    Output.Write($"{FormatEntry(entries[index].EntryType)} {entries[index].Message}")
                        .WithBackgroundColour(ConsoleColor.Gray).WithForegroundColour(ConsoleColor.Black).Run();

                }

                break;
            case (ConsoleKey.M):
                if (index >= 0)
                {
                    if (IsTodo(entries[index].EntryType))
                    {
                        await new UpdateHandler(db).Handle(new Update(entries[index].Id, entries[index].Date,
                            EntryType.TodoMigrated), cancellationSource.Token);
                        
                        entries = await new GetHandler(db).Handle(new Get(date), cancellationSource.Token);
                    }

                    Console.SetCursorPosition(0, origRow + currentRow);
                    Output.Write($"{FormatEntry(entries[index].EntryType)} {entries[index].Message}")
                        .WithBackgroundColour(ConsoleColor.Gray).WithForegroundColour(ConsoleColor.Black).Run();

                }

                break;
            case (ConsoleKey.Spacebar):
                var loop = new CircularList<EntryType>(new[] { EntryType.Todo, EntryType.Event, EntryType.Note });
                var entryType = loop.Current();
                Console.SetCursorPosition(0, Console.WindowHeight - 2);
                for (var i = 0; i < windowWidth; i++)
                {
                    Console.Write('-');
                }

                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                for (var i = 0; i < windowWidth-1; i++)
                {
                    Console.Write(' ');
                }

                Console.SetCursorPosition(0, Console.WindowHeight - 1 );
                Console.Write($@"{FormatEntry(entryType)} : ");
                Console.CursorVisible = true;
                Console.CursorVisible = true;

                var exit2 = false;
                var entry = new StringBuilder();
                while (!exit2)
                {
                    var key2 = Console.ReadKey(true);
                    var currentLeft = Console.CursorLeft;
                    switch (key2.Key)
                    {
                        case (ConsoleKey.UpArrow):
                            entryType = loop.Previous();

                            Console.SetCursorPosition(0, Console.WindowHeight - 1);
                            Console.Write($@"{FormatEntry(entryType)} : ");
                            Console.SetCursorPosition(currentLeft, Console.WindowHeight - 1);
                            break;
                        case (ConsoleKey.DownArrow):
                            entryType = loop.Next();
                            Console.SetCursorPosition(0, Console.WindowHeight - 1);
                            Console.Write($@"{FormatEntry(entryType)} : ");
                            Console.SetCursorPosition(currentLeft, Console.WindowHeight - 1);
                            break;

                        case (ConsoleKey.Enter):
                            await new AddHandler(db).Handle(new Add(entry.ToString(), entryType, date), cancellationSource.Token);
                            entries =  await new GetHandler(db).Handle(new Get(date), cancellationSource.Token);
                            Draw(origRow, origCol, entries);
                            currentRow = 1;
                            index = -1;
                            exit2 = true;
                            break;
                        case (ConsoleKey.Backspace):
                            if (currentLeft > 4)
                            {
                                Console.SetCursorPosition(currentLeft - 1, Console.WindowHeight - 1);
                                Console.Write(' ');
                                Console.SetCursorPosition(currentLeft - 1, Console.WindowHeight - 1);
                                entry.Remove(entry.Length - 1, 1);
                            }

                            break;
                        default:
                            if (Char.IsControl(key2.KeyChar))
                            {
                                break;
                            }

                            entry.Append(key2.KeyChar);
                            Console.Write(key2.KeyChar);
                            break;

                    }
                }

                break;



        }
    }

});

await rootCommand.InvokeAsync(args);


