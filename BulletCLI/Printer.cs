using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using BulletCLI.Model;
using BulletCLI.Todos;

namespace BulletCLI;

public class ConsolePrinter : IObserver<Task<DailyTodoList>>, IDisposable
{
    private const int HeaderRows = 2;
    private const int FooterRows = 2;
    private readonly int _lineHeight;
    

    private int TodoWindowHeight() => _lineHeight - FooterRows - HeaderRows;
    
    public ConsolePrinter()
    {
        _lineHeight = Console.WindowHeight;
        Console.OutputEncoding = Encoding.UTF8; 
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.SetBufferSize(1000, 1000);
        }

        Console.CursorVisible = false;
        FillRow(1,'-');
        FillRow(HeaderRows + TodoWindowHeight() ,'-');
    }
    
    private static void FillRow(int top, char character)
    {
        var currentTop = Console.CursorTop;
        var currentLeft = Console.CursorLeft;
        
        
        Console.SetCursorPosition(0, top);
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
    
    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    private DateOnly _currentDate = DateOnly.MinValue;
    private IList<(TodoDto todo, bool selected)> _previousTodoEntries = new List<(TodoDto todo, bool selected)>();


    public async void OnNext(Task<DailyTodoList> todoListTask)
    {
        var (entries, dateOnly) = await todoListTask;
        
        if (_currentDate != dateOnly)
        {
            _currentDate = dateOnly;

            Console.SetCursorPosition(0, 0);
            FillRow(0,' ');
            Console.WriteLine($"{_currentDate:D}");
            

            for (int i = 0; i < TodoWindowHeight(); i++)
            {
                FillRow(i+HeaderRows, ' ');
            }
        }
        
        Console.SetCursorPosition(0,2);
        for (int i =0; i<entries.Count; i++)
        {
            if (_previousTodoEntries.Count > i && _previousTodoEntries[i].todo == entries[i].todo && _previousTodoEntries[i].selected == entries[i].selected)
            {

            }
            else
            {
                FillRow(HeaderRows+i, ' ');
                Console.SetCursorPosition(0,HeaderRows+i);

                Output.Write($"{FormatEntry(entries[i].todo.EntryType)} {entries[i].todo.Message}")
                    .WithBackgroundColour(entries[i].selected ? ConsoleColor.Gray : Console.BackgroundColor)
                    .WithForegroundColour(entries[i].selected ? ConsoleColor.Black : Console.ForegroundColor).Run();
                Console.WriteLine();
            }
        }

        _currentDate = dateOnly;
        _previousTodoEntries = entries;
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
            Console.CursorVisible = true;
        }
        
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
                    FillRow(_lineHeight-1,' ');
                    Console.CursorVisible = false;
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