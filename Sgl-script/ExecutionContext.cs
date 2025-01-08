namespace Sgl_script;

public class ExecutionContext
{
    private Dictionary<string, Func<List<object>, object>> _functions = new(LanguageSpecification.StandardFunctions);
    private Dictionary<string, Func<object, List<object>, object>> _methods = new(LanguageSpecification.StandardMethods);

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
}