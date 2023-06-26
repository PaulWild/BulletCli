using System.Runtime.InteropServices;
using System.Text;
using BulletCLI.Model;
using BulletCLI.Todos;

namespace BulletCLI;

public class ConsolePrinter : IObserver<IList<(TodoDto todo, bool selected)>>, IDisposable
{
    private static void FillRow(char character)
    {
        for (var i = 0; i < (Console.WindowWidth); i++)
        {
            Console.Write(character);
        }
    }

    private static string FormatEntry(EntryType type)
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

    public ConsolePrinter()
    {
        Console.OutputEncoding = Encoding.UTF8; 
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.SetBufferSize(1000, 1000);
        }
    }
    
    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnNext(IList<(TodoDto todo, bool selected)> entries)
    {
        Console.CursorVisible = false;
        var date = entries[0];
        
        Console.Clear();
        Console.WriteLine($"{date:D}");
        FillRow('-');
    
        Console.WriteLine();

        foreach (var entry in entries)
        {
            Output.Write($"{FormatEntry(entry.todo.EntryType)} {entry.todo.Message}")
                .WithBackgroundColour(entry.selected ? ConsoleColor.Gray : Console.BackgroundColor)
                .WithForegroundColour(entry.selected ? ConsoleColor.Black : Console.ForegroundColor).Run();
            Console.WriteLine($"{FormatEntry(entry.todo.EntryType)} {entry.todo.Message}");
        }
    }

    public void Dispose()
    {
        Console.CursorVisible = true;
    }
}