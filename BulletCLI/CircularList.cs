namespace BulletCLI;

public class CircularList<T>
{
    private readonly LinkedList<T> _items;
    private LinkedListNode<T>? _current;

    public CircularList(IEnumerable<T> items )
    {
        _items = new LinkedList<T>(items);
        if (_items.First != null) _current = _items.First;
    }

    public T Current()
    {
        if (_current != null) return _current.Value;

        throw new Exception();
    }

    public T Next()
    {
        _current = _current?.Next ?? _items.First;

        if (_current != null) return _current.Value;

        throw new Exception();
    }
    
    public T Previous()
    {
        _current = _current?.Previous ?? _items.Last;

        if (_current != null) return _current.Value;

        throw new Exception();
    }
}