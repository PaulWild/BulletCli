namespace BulletCLI;

public class Output
{
    private Output(string write)
    {
        _toWrite = write;
    }
    
    private readonly string _toWrite;

    private readonly List<Func<Action,Action>> _decorations = new();

    public Output WithForegroundColour(ConsoleColor color)
    {
        _decorations.Add(a =>
        {
            return () =>
            {
                var prev = Console.ForegroundColor;
                Console.ForegroundColor = color;
                a();
                Console.ForegroundColor = prev;
            };
        });

        return this;
    }
    
    public Output WithBackgroundColour(ConsoleColor color)
    {
        _decorations.Add(a =>
        {
            return () =>
            {
                var prev = Console.BackgroundColor;
                Console.BackgroundColor = color;
                a();
                Console.BackgroundColor = prev;
            };
        });

        return this;
    }

    public void Run()
    {
        void Action()
        {
            Console.Write(_toWrite);
        }

        var t = _decorations
            .Aggregate((Action) Action, (agg, nxt) => nxt(agg));

        t();

    }

    public static Output Write(string write)
    {
        return new(write);
    }
    
    
}
