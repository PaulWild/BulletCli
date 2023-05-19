using BulletCLI.Model;
using MediatR;

namespace BulletCLI.Todos;

public record Add(string Message, EntryType EntryType, DateOnly Date) : IRequest;

public class AddHandler : IRequestHandler<Add>
{
    private readonly TodoContext _db;

    public AddHandler(TodoContext db)
    {
        _db = db;
    }
    
    public async Task Handle(Add request, CancellationToken cancellationToken)
    {
        var todo = new Todo() { Detail = request.Message };
        var todoEvent = new TodoEvent() { EntryType = request.EntryType, Todo = todo, Date = request.Date};

        await _db.AddAsync(todo, cancellationToken);
        await _db.AddAsync(todoEvent, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
    }
}