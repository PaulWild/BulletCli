using Microsoft.EntityFrameworkCore;

namespace BulletCLI.Model;


public class TodoContext : DbContext
{
    public DbSet<Todo> Todo { get; set; }
    public DbSet<TodoEvent> TodoEvents { get; set; }
    
    public string DbPath { get; }

    public TodoContext()
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var localPath = Environment.GetFolderPath(folder);
        var fullPath = Path.Combine(localPath, "BulletCli");

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
        
        DbPath = Path.Join(fullPath, "bullet.db");
    }
    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

public enum EntryType
{
    Todo,
    Event,
    Note,
    TodoMigrated,
    TodoDone
}

public class Todo
{
    public int TodoId { get; set; }
    public string Detail { get; set; }
    public List<TodoEvent> TodoEvents { get; set; }
}

public class TodoEvent
{
    public int TodoEventId { get; set; }
    public EntryType EntryType { get; set; }
    
    public DateOnly Date { get; set; }

    public int TodoId { get; set; }
    public Todo Todo { get; set; }
}

public record Entry(EntryType EntryType, string Content, DateOnly Date);
