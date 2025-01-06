namespace Sgl_script;

public class ProgramVisualizer : Ast.IVisitor
{
    private string _indent = "";
    private bool _last;
    
    public void Print(Ast ast)
    {
        Console.WriteLine("Program");
        for (int i = 0; i < ast.Statements.Count; i++)
        {
            Ast.INode statement = ast.Statements[i];
            _indent = "";
            _last = i == ast.Statements.Count - 1;
            Print(statement);
        }
    }
    
    private void Print(Ast.INode node)
    {
        Console.Write(_indent);
        if (_last)
        {
            Console.Write("└─");
            _indent += "  ";
        }
        else
        {
            Console.Write("├─");
            _indent += "| ";
        }
        node.Accept(this);
    }
    
    public void Visit<T>(Ast.Nodes.Literal<T> node)
    {
        Console.WriteLine(node);
    }

    public void Visit(Ast.Nodes.Variable node)
    {
        Console.WriteLine(node.Name);
    }

    public void Visit(Ast.Nodes.BinaryOperator node)
    {
        Console.WriteLine(node.Operator);
        
        _last = false;
        string tmp = _indent;
        Print(node.Left);
        
        _last = true;
        _indent = tmp;
        Print(node.Right);
    }

    public void Visit(Ast.Nodes.Assignment node)
    {
        Console.WriteLine(node.Name + " = ");
        _indent += "  ";
        _last = true;
        Print(node.Expression);
    }

    public void Visit(Ast.Nodes.FunctionCall node)
    {
        Console.WriteLine(node.Name);
        for (int i = 0; i < node.Arguments.Count; i++)
        {
            Ast.INode argument = node.Arguments[i];
            _last = i == node.Arguments.Count - 1;
            Print(argument);
        }
    }
}