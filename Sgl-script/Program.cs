using System.Globalization;

namespace Sgl_script;

class Program
{
    public static void Main(string[] args)
    {
        using var file = new FileStream("test.sgl", FileMode.Open);
        var reader = new StreamReader(file);
        var lexer = new Lexer(reader.ReadToEnd());
        var parser = new Parser(lexer);
        Ast ast = parser.Parse();
        for (int i = 0; i < ast.Statements.Count; i++)
        {
            Ast.INode statement = ast.Statements[i];
            Print(statement, "", i == ast.Statements.Count - 1);
        }
    }

    private static void Print(Ast.INode node, string indent, bool last)
    {
        Console.Write(indent);
        if (last)
        {
            Console.Write("└─");
            indent += "  ";
        }
        else
        {
            Console.Write("├─");
            indent += "| ";
        }
        switch (node)
        {
            case Ast.Nodes.Literal<double> or Ast.Nodes.Literal<string>:
                Console.WriteLine(node);
                break;
            case Ast.Nodes.Variable variable:
                Console.WriteLine(variable.Name);
                break;
            
            case Ast.Nodes.Assignment assignment:
                Console.WriteLine(assignment.Name + " = ");
                indent += "  ";
                Print(assignment.Value, indent, true);
                break;
            
            case Ast.Nodes.BinaryOperator binaryOperator:
                Console.WriteLine(binaryOperator.Operator);
                Print(binaryOperator.Left, indent, false);
                Print(binaryOperator.Right, indent, true);
                break;
            
            case Ast.Nodes.FunctionCall functionCall:
                Console.WriteLine(functionCall.Name);
                for (int i = 0; i < functionCall.Arguments.Count; i++)
                {
                    Ast.INode argument = functionCall.Arguments[i];
                    Print(argument, indent, i == functionCall.Arguments.Count - 1);
                }
                break;
        }
    }
}