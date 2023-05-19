using BulletCLI.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BulletCLI.Todos;

public record Update(int TodoId, DateOnly Date, EntryType EntryType) : IRequest;

public class UpdateHandler : IRequestHandler<Update>
{
    private readonly TodoContext _db;

    public UpdateHandler(TodoContext db)
    {
        _db = db;
    }
    
    public async Task Handle(Update request, CancellationToken cancellationToken)
    {
        var toUpdate = await _db.TodoEvents.Include(x => x.Todo)
            .FirstAsync(todoEvent => todoEvent.TodoId == request.TodoId && todoEvent.Date == request.Date, cancellationToken: cancellationToken);

        if (toUpdate.EntryType == request.EntryType)
        {
            return;
        }

        switch (toUpdate.EntryType)
        {
            case EntryType.TodoMigrated:
            case EntryType.Note:
            case EntryType.Event:
                return;
            case EntryType.Todo:
                break;
            case EntryType.TodoDone:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


        toUpdate.EntryType = request.EntryType;
        
        
        if (request.EntryType == EntryType.TodoMigrated)
        {
            toUpdate.Todo.TodoEvents.Add(new TodoEvent
            {
                Date = toUpdate.Date.AddDays(1),
                EntryType = EntryType.Todo
            });
        }
        
        await _db.SaveChangesAsync(cancellationToken);
    }
}