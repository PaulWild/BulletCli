using BulletCLI.Model;
using BulletCLI.Todos;

namespace BulletCLI;

public class Entries : IObservable<IList<(TodoDto todo, bool selected)>>
{
    private Task<IList<TodoDto>> _todos;
    private DateOnly _date;
    private readonly CancellationToken _cancellationToken;
    private readonly List<IObserver<IList<(TodoDto todo, bool selected)>>> _observers = new();
    private readonly UpdateHandler _updateHandler;
    private readonly AddHandler _addHandler;

    public Entries(DateOnly date, CancellationToken cancellationToken)
    {
        _date = date;
        _todos = new GetHandler().Handle(new Get(date), cancellationToken);
        _cancellationToken = cancellationToken;
        _addHandler = new AddHandler();
        _updateHandler = new UpdateHandler();
    }

    private int? CurrentSelectedIndex { get; set; }

    public async Task SelectNext()
    {
        if (CurrentSelectedIndex == null)
        {
            CurrentSelectedIndex = 0;
        }
        else if (CurrentSelectedIndex < (await _todos).Count)
        {
            CurrentSelectedIndex++;
        }

        await Update();
    }

    public async Task SelectPrevious()
    {
        if (CurrentSelectedIndex == 0)
        {
            CurrentSelectedIndex = 0;
        }
        else if (CurrentSelectedIndex < (await _todos).Count)
        {
            CurrentSelectedIndex--;
        } 
        
        await Update();
    }

    public async Task ChangeDate(DateOnly date)
    {
        _date = date;
        _todos = new GetHandler().Handle(new Get(date), _cancellationToken);
        await Update();
    }

    public async Task AddEntry(EntryType type, string message)
    {
        await _addHandler.Handle(new Add(message, type, _date), _cancellationToken);
        await Update();
    }
    
    
    public async Task UpdateEntry(int todoId, EntryType type)
    {
        await _updateHandler.Handle(new Update(todoId, _date, type), _cancellationToken);
        await Update();
    }
    
    private class Unsubscriber : IDisposable
    {
        private readonly List<IObserver<IList<(TodoDto todo, bool selected)>>> _observers;
        private readonly IObserver<IList<(TodoDto todo, bool selected)>> _observer;

        public Unsubscriber(List<IObserver<IList<(TodoDto todo, bool selected)>>> observers, IObserver<IList<(TodoDto todo, bool selected)>> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            _observers.Remove(_observer);
        }
    }


    public IDisposable Subscribe(IObserver<IList<(TodoDto todo, bool selected)>> observer)
    {
        if (! _observers.Contains(observer))
            _observers.Add(observer);

        return new Unsubscriber(_observers, observer);
    }
    
    private async Task Update()
    {
        var toReturn = new List<(TodoDto todo, bool selected)>();
        for (var i =0; i<(await _todos).Count; i++)
        {
            toReturn.Add(((await _todos)[i], i==CurrentSelectedIndex));
        }

        foreach (var observer in _observers)
        {
            observer.OnNext(toReturn);
        
        }
    }
}