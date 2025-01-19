namespace Sgl_script;

class Program
{
    public static void Main(string[] args)
    {
        using var file = new FileStream(args[0], FileMode.Open);
        var reader = new StreamReader(file);
        
        var context = new ExecutionContext();
        foreach (string arg in args.Skip(1))
        {
            context.AddArgument(arg);
        }
        
        var lexer = new Lexer(reader.ReadToEnd());
        var parser = new Parser(lexer, context);
        var ast = parser.Parse();
        
        // var visualizer = new ProgramVisualizer();
        // visualizer.Print(ast);
        
        var interpreter = new Interpreter(context);
        interpreter.Run(ast);
    }
}