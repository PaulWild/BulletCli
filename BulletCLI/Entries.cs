using BulletCLI.Model;
using BulletCLI.Todos;

namespace BulletCLI;

public record DailyTodoList(IList<(TodoDto todo, bool selected)> Entries, DateOnly Date);
public class Entries : IObservable<Task<DailyTodoList>>
{
    private Task<IList<TodoDto>> _todos;
    private DateOnly _date;
    private readonly CancellationToken _cancellationToken;
    private readonly List<IObserver<Task<DailyTodoList>>> _observers = new();
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
        else if (CurrentSelectedIndex < (await _todos).Count-1)
        {
            CurrentSelectedIndex++;
        }

        NotifyObservers();
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
        
        NotifyObservers();
    }

    public async Task ChangeDate(DateOnly date)
    {
        _date = date;
        CurrentSelectedIndex = null;
        await RefreshData();
    }
    
    private async Task RefreshData()
    {
        _todos = new GetHandler().Handle(new Get(_date), _cancellationToken);
        NotifyObservers();
    }

    public async Task AddEntry(EntryType type, string message)
    {
        await _addHandler.Handle(new Add(message, type, _date), _cancellationToken);
        await RefreshData();
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

    
    public async Task UpdateEntry(EntryType type)
    {
        if (CurrentSelectedIndex != null && IsTodo(type))
        {
            await _updateHandler.Handle(new Update((await _todos)[CurrentSelectedIndex.Value].Id, _date, type),
                _cancellationToken);
        }

        await RefreshData();
    }
    
    private class Unsubscriber : IDisposable
    {
        private readonly List<IObserver<Task<DailyTodoList>>> _observers;
        private readonly IObserver<Task<DailyTodoList>> _observer;

        public Unsubscriber(List<IObserver<Task<DailyTodoList>>> observers, IObserver<Task<DailyTodoList>> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            _observers.Remove(_observer);
        }
    }


    public IDisposable Subscribe(IObserver<Task<DailyTodoList>> observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
            observer.OnNext(GetObservableData());
        }

        return new Unsubscriber(_observers, observer);
    }
    
    private void NotifyObservers()
    {
        var toReturn = GetObservableData();

        foreach (var observer in _observers)
        {
            observer.OnNext(toReturn);
        }
    }

    private async Task<DailyTodoList> GetObservableData()
    {
        var todoList = new List<(TodoDto todo, bool selected)>();
        for (var i = 0; i < (await _todos).Count; i++)
        {
            todoList.Add(((await _todos)[i], i == CurrentSelectedIndex));
        }

        return new DailyTodoList(todoList, _date);
    }
}