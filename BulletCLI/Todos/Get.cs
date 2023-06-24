using System.Data;
using BulletCLI.Model;
using Dapper;

namespace BulletCLI.Todos;

public record Get(DateOnly Date);

public record TodoDto
{
    public int Id { get; init; } 
    public string Message { get; init; } 
    public EntryType EntryType { get; init; } 
    public DateOnly Date { get; init; }
}

public class GetHandler
{
    private const string Sql = @"
SELECT
    Todo.TodoID as Id,
    Detail as Message,
	TodoEvents.EntryType,
	TodoEvents.Date
FROM Todo
INNER JOIN TodoEvents ON Todo.TodoId == TodoEvents.TodoId
WHERE TodoEvents.Date == @Date";

    public async Task<IList<TodoDto>> Handle(Get request, CancellationToken cancellationToken)
    {
        await using var connection = TodoContext.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        
        return (await connection.QueryAsync<TodoDto>(Sql, new { request.Date })).ToList();
    }
}

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value) => DateOnly.FromDateTime(DateTime.Parse((string)value));


    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString("yyyy-MM-dd");
    }
}
