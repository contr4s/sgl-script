namespace Sgl_script;

public class MemoryManager
{
    private const long MaxStackDepth = 100000;
    
    private readonly Dictionary<string, ManagedObject> _globalMemory = new();
    private readonly Stack<List<string>> _callStack = new();
    
    public void Allocate(string name, object value)
    {
        if (_callStack.Count > 0)
        {
            _callStack.Peek().Add(name);
        }
        if (_globalMemory.ContainsKey(name))
        {
            ReleaseReference(name);
        }
        _globalMemory[name] = new ManagedObject(value);
    }

    public void AddReference(string name)
    {
        if (_globalMemory.TryGetValue(name, out ManagedObject? entry))
        {
            entry.RefCount++;
        }
        else
        {
            throw LanguageException.RuntimeError($"Variable {name} does not exist");
        }
    }

    public void ReleaseReference(string name)
    {
        if (_globalMemory.TryGetValue(name, out ManagedObject? entry))
        {
            entry.RefCount--;
            if (entry.RefCount <= 0)
            {
                if (_callStack.Count > 0 && _callStack.Peek().Contains(name))
                {
                    _callStack.Peek().Remove(name);
                }
                _globalMemory.Remove(name);
            }
        }
        else
        {
            throw LanguageException.RuntimeError($"Variable {name} does not exist");
        }
    }

    public void EnterScope()
    {
        if (_callStack.Count >= MaxStackDepth)
        {
            throw LanguageException.RuntimeError("Maximum recursion depth exceeded");
        }

        _callStack.Push([]);
    }
    
    public void ExitScope()
    {
        var variables = _callStack.Pop();
        foreach (string name in variables)
        {
            if (_globalMemory.TryGetValue(name, out ManagedObject? entry))
            {
                entry.RefCount--;
            }
        }
    }
    
    public Object GetValue(string name) => _globalMemory.GetValueOrDefault(name)?.Data ?? throw LanguageException.RuntimeError($"Variable {name} does not exist");

    private class ManagedObject(object data)
    {
        public object Data { get; set; } = data;
        public int RefCount { get; set; }
    }
}