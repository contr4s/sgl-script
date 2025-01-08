namespace Sgl_script;

public class LanguageException : Exception
{
    private LanguageException(string message) : base(message)
    {
    }
    
    public static LanguageException SyntaxError(int line, string message) => new LanguageException($"Syntax error on line {line}: {message}");
    
    public static LanguageException RuntimeError(string message) => new LanguageException($"Runtime error: {message}");
    
    public static LanguageException ContextError(string message) => new LanguageException($"Context creation error: {message}");
}