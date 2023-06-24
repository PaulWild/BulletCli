using System.Data.Common;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;

namespace BulletCLI.Model;

public class TodoContext 
{
    public static DbConnection GetDbConnection()
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var localPath = Environment.GetFolderPath(folder);
        var fullPath = Path.Combine(localPath, "BulletCli");

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
        
        var path = Path.Join(fullPath, "bullet.db");
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = path
        }.ToString();
        return new SqliteConnection(connectionString);

    }
}

public enum EntryType
{
    Todo,
    Event,
    Note,
    TodoMigrated,
    TodoDone
}

[Table("Todo")]
public class Todo
{
    [Key]
    public int TodoId { get; set; }
    public string? Detail { get; set; }
}

public class TodoEvent
{
    [Key]
    public int TodoEventId { get; set; }
    public EntryType EntryType { get; set; }
    public DateOnly Date { get; set; }
    public int TodoId { get; set; }

}
