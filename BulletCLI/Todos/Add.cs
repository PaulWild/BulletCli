using BulletCLI.Model;
using Dapper.Contrib.Extensions;

namespace BulletCLI.Todos;

public record Add(string Message, EntryType EntryType, DateOnly Date);

public class AddHandler
{
    public async Task Handle(Add request, CancellationToken cancellationToken)
    {
        await using var connection = TodoContext.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        await using var  transaction = await connection.BeginTransactionAsync(cancellationToken);

        var id = await connection.InsertAsync(new Todo { Detail = request.Message });
        await connection.InsertAsync(new TodoEvent { EntryType = request.EntryType, Date = request.Date, TodoId = id});

        await transaction.CommitAsync(cancellationToken);

    }
}