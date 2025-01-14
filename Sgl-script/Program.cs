﻿namespace Sgl_script;

class Program
{
    public static void Main(string[] args)
    {
        using var file = new FileStream("test.sgl", FileMode.Open);
        var reader = new StreamReader(file);
        var context = new ExecutionContext();
        var lexer = new Lexer(reader.ReadToEnd());
        var parser = new Parser(lexer, context);
        var ast = parser.Parse();
        var visualizer = new ProgramVisualizer();
        visualizer.Print(ast);
        var interpreter = new Interpreter(context);
        interpreter.Run(ast);
    }
}