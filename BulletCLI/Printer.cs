using System.Runtime.InteropServices;
using System.Text;
using BulletCLI.Model;

namespace BulletCLI;

public class ConsolePrinter : IObserver<Task<DailyTodoList>>, IDisposable
{
    private static void FillRow(int left, char character)
    {
        var currentTop = Console.CursorTop;
        var currentLeft = Console.CursorLeft;
        
        Console.SetCursorPosition(0, left);
        for (var i = 0; i < Console.WindowWidth; i++)
        {
            Console.Write(character);
        }
        Console.SetCursorPosition(currentTop, currentLeft);
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

        Console.CursorVisible = false;
        FillRow(1,'-');
    }
    
    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    private DateOnly _currentDate = DateOnly.MinValue; 

    public async void OnNext(Task<DailyTodoList> todoListTask)
    {
        var todoList = await todoListTask;
        Console.CursorVisible = false;
        if (_currentDate != todoList.Date)
        {
            _currentDate = todoList.Date;

            Console.SetCursorPosition(0, 0);
            FillRow(0,' ');
            Console.WriteLine($"{_currentDate:D}");
        }
        
        for (var top = 2; top<Console.WindowHeight; top++)
        {
            FillRow(top, ' ');
        }
        
        Console.SetCursorPosition(0,2);
        foreach (var entry in todoList.Entries)
        {
            Output.Write($"{FormatEntry(entry.todo.EntryType)} {entry.todo.Message}")
                .WithBackgroundColour(entry.selected ? ConsoleColor.Gray : Console.BackgroundColor)
                .WithForegroundColour(entry.selected ? ConsoleColor.Black : Console.ForegroundColor).Run();
            Console.WriteLine();
        }
    }

    public void EditMode(Func<EntryType, string, Task> onSubmit)
    {
        var loop = new CircularList<EntryType>(new[] { EntryType.Todo, EntryType.Event, EntryType.Note });
        var entryType = loop.Current();

        void UpdateEntryType()
        {
            var currentLeft = Console.CursorLeft;
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write($@"{FormatEntry(entryType)} : ");
            Console.SetCursorPosition(currentLeft, Console.WindowHeight - 1);
        }
        
        FillRow(Console.WindowHeight - 2,  '-');
        UpdateEntryType();
        Console.SetCursorPosition(4, Console.WindowHeight - 1);
        

        var entry = new StringBuilder();
        while (true)
        {
            var key2 = Console.ReadKey(true);
            var currentLeft = Console.CursorLeft;
            switch (key2.Key)
            {
                case ConsoleKey.UpArrow:
                    entryType = loop.Previous();
                    UpdateEntryType();
                    break;
                case ConsoleKey.DownArrow:
                    entryType = loop.Next();
                    UpdateEntryType();
                    break;

                case ConsoleKey.Enter:
                    onSubmit(entryType,entry.ToString());
                    return;
                case ConsoleKey.Backspace:
                    if (currentLeft > 4)
                    {
                        Console.SetCursorPosition(currentLeft - 1, Console.WindowHeight - 1);
                        Console.Write(' ');
                        Console.SetCursorPosition(currentLeft - 1, Console.WindowHeight - 1);
                        entry.Remove(entry.Length - 1, 1);
                    }

                    break;
                default:
                    if (char.IsControl(key2.KeyChar))
                    {
                        break;
                    }

                    entry.Append(key2.KeyChar);
                    Console.Write(key2.KeyChar);
                    break;
            }
        }
    }

    public void Dispose()
    {
        Console.CursorVisible = true;
    }
}