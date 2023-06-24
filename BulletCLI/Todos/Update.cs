using BulletCLI.Model;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BulletCLI.Todos;

public record Update(int TodoId, DateOnly Date, EntryType EntryType);

public class UpdateHandler 
{
    private const string Sql = @"
SELECT
    TodoEventId,
    EntryType,
    Date,
    TodoId
FROM TodoEvents
WHERE TodoId = @TodoId AND Date = @Date";
    
    public async Task Handle(Update request, CancellationToken cancellationToken)
    {
        await using var connection = TodoContext.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        await using var  transaction = await connection.BeginTransactionAsync(cancellationToken);

        var todoEvent = await connection.QuerySingleAsync<TodoEvent>(Sql, new { request.TodoId, request.Date });
        
        
        if (todoEvent.EntryType == request.EntryType)
        {
            return;
        }

        switch (todoEvent.EntryType)
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
                throw new ArgumentOutOfRangeException(todoEvent.EntryType.ToString());
        }


        todoEvent.EntryType = request.EntryType;

        await connection.UpdateAsync(todoEvent);
        if (request.EntryType == EntryType.TodoMigrated)
        {
            await connection.InsertAsync(new TodoEvent
            {
                Date = todoEvent.Date.AddDays(1),
                EntryType = EntryType.Todo,
                TodoId = todoEvent.TodoId
            });
        }
        await transaction.CommitAsync(cancellationToken);
    }
}