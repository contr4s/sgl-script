namespace Sgl_script;

public class ProgramVisualizer : Ast.IVisitor
{
    private string _indent = "";
    private bool _last;
    
    public void Print(Ast ast)
    {
        Console.WriteLine("Program");
        _indent = "";
        ast.Root.Accept(this);
    }
    
    private void Print(Ast.INode node)
    {
        Print();
        node.Accept(this);
    }

    private void Print()
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
    }
    
    public void Visit<T>(Ast.Nodes.Literal<T> node)
    {
        Console.WriteLine(node);
    }

    public void Visit(Ast.Nodes.Array node)
    {
        Console.WriteLine("Array");
        _indent += "  ";
        string tmp = _indent;
        for (int i = 0; i < node.Elements.Count; i++)
        {
            Ast.INode child = node.Elements[i];
            _last = i == node.Elements.Count - 1;
            _indent = tmp;
            Print(child);
        }
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

    public void Visit(Ast.Nodes.UnaryOperator node)
    {
        Console.WriteLine(node.Operator);
        _last = true;
        Print(node.Expression);
    }

    public void Visit(Ast.Nodes.Assignment node)
    {
        Console.WriteLine(node.Variable + " = ");
        _indent += "  ";
        _last = true;
        Print(node.Expression);
    }

    public void Visit(Ast.Nodes.FunctionCall node)
    {
        Console.WriteLine(node.Name);
        string tmp = _indent;
        for (int i = 0; i < node.Arguments.Count; i++)
        {
            Ast.INode argument = node.Arguments[i];
            _last = i == node.Arguments.Count - 1;
            _indent = tmp;
            Print(argument);
        }
    }

    public void Visit(Ast.Nodes.MethodCall node)
    {
        Console.WriteLine($"{node.Variable.Name}.{node.Name}");
        string tmp = _indent;
        for (int i = 0; i < node.Arguments.Count; i++)
        {
            Ast.INode argument = node.Arguments[i];
            _last = i == node.Arguments.Count - 1;
            _indent = tmp;
            Print(argument);
        }
    }

    public void Visit(Ast.Nodes.Conditional node)
    {
        Console.WriteLine("Conditional");
        _indent += "  ";
        
        _last = false;
        string tmp = _indent;
        Print(node.Condition);
        
        bool hasElse = node.FalseBranch is not null;
        _indent = tmp;
        _last = !hasElse;
        Print();
        Console.WriteLine("True branch");
        node.TrueBranch.Accept(this);
        
        if (hasElse)
        {
            _indent = tmp;
            Print();
            Console.WriteLine("False branch");
            node.FalseBranch.Accept(this);
        }
    }

    public void Visit(Ast.Nodes.Compound node)
    {
        string tmp = _indent;
        for (int i = 0; i < node.Statements.Count; i++)
        {
            Ast.INode statement = node.Statements[i];
            _indent = tmp;
            _last = i == node.Statements.Count - 1;
            Print(statement);
        }
    }

    public void Visit(Ast.Nodes.Loop node)
    {
        Console.WriteLine($"Loop on {node.IteratorName}");
        _indent += "  ";
        _last = false;
        string tmp = _indent;
        
        Print();
        _last = true;
        Console.WriteLine("Iterable");
        _indent += "  ";
        Print(node.Iterable);
        
        _last = true;
        _indent = tmp;
        Print();
        Console.WriteLine("Body");
        node.Body.Accept(this);
    }
}