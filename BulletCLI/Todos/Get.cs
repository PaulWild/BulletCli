using BulletCLI.Model;
using Microsoft.EntityFrameworkCore;

namespace BulletCLI.Todos;

public record Get(DateOnly Date);

public record TodoDto(int Id, string Message, EntryType EntryType, DateOnly Date);

public class GetHandler
{
    private readonly TodoContext _db;

    public GetHandler(TodoContext db)
    {
        _db = db;
    }
    
    public async Task<IList<TodoDto>> Handle(Get request, CancellationToken cancellationToken)
    {
        return await _db.TodoEvents
            .Where(x => x.Date == request.Date)
            .Select(x => new TodoDto(x.TodoId, x.Todo.Detail, x.EntryType, x.Date))
            .ToListAsync(cancellationToken);
    }
}