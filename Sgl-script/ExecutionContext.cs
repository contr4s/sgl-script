using System.Globalization;

namespace Sgl_script;

public class ExecutionContext
{
    private readonly Dictionary<string, Func<List<object>, object>> _functions = new(LanguageSpecification.StandardFunctions);
    private readonly Dictionary<string, Func<object, List<object>, object>> _methods = new(LanguageSpecification.StandardMethods);
    
    private readonly Queue<object> _arguments = [];

    public void RegisterFunction(string name, Func<List<object>, object> function)
    {
        if (!_functions.TryAdd(name, function))
            throw LanguageException.ContextError($"function already exists: {name}");
    }
    public bool HasFunction(string name) => _functions.ContainsKey(name);
    public object? ExecuteFunction(string name, List<object> arguments) => _functions[name](arguments);

    public void RegisterMethod(string name, Func<object, List<object>, object> method)
    {
        if (!_methods.TryAdd(name, method))
            throw LanguageException.ContextError($"method already exists: {name}");
    }
    public bool HasMethod(string name) => _methods.ContainsKey(name);
    public object? ExecuteMethod(object obj, string name, List<object> arguments) => _methods[name](obj, arguments);
    
    public void AddArgument(object arg) => _arguments.Enqueue(arg);

    public T ConsumeArgument<T>()
    {
        object arg = _arguments.Dequeue();
        if (arg is T t)
            return t;

        if (arg is IConvertible convertible)
        {
            return (T)convertible.ToType(typeof(T), CultureInfo.InvariantCulture);
        }
        
        throw LanguageException.ContextError($"argument is not of type {typeof(T)}");
    }

    public List<object> ConsumeArray()
    {
        object arg = _arguments.Dequeue();
        if (arg is List<object> t)
            return t;

        if (arg is not string str)
            throw LanguageException.ContextError("argument is not array");

        return str[1..^1].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => double.TryParse(x, CultureInfo.InvariantCulture, out double val) ? (object)val : x).ToList();
    }
}