using System.Runtime.InteropServices;

namespace BulletCLI;

record Position(int left, int top);

public class Terminal : IDisposable
{
    private Position cursorLocation;
    public Terminal()
    {
        cursorLocation = new Position(Console.CursorLeft, Console.CursorTop);
        Console.CursorVisible = false;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.SetBufferSize(1000, 1000);
        }
    }

    public void Dispose()
    {
        Console.CursorVisible = true;
    }
}

public class Section
{
    public Section()
    {
        
    }
}