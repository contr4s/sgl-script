namespace Sgl_script;

public class Interpreter : Ast.IVisitor
{
    private Dictionary<string, object> _variables = new();
    private Dictionary<string, Func<List<object>, object>> _functions = new(LanguageSpecification.StandardFunctions);
    
    private Stack<object> _stack = new();

    public void Run(Ast ast)
    {
        ast.Root.Accept(this);
    }

    public void Visit<T>(Ast.Nodes.Literal<T> node)
    {
        if (node.Value is not null)
            _stack.Push(node.Value);
    }

    public void Visit(Ast.Nodes.Variable node)
    {
        if (_variables.TryGetValue(node.Name, out var value))
            _stack.Push(value);
        else
            throw LanguageException.RuntimeError($"Variable {node.Name} is not defined");
    }

    public void Visit(Ast.Nodes.BinaryOperator node)
    {
        node.Left.Accept(this);
        var left = _stack.Pop();
        node.Right.Accept(this);
        var right = _stack.Pop();

        object res = node.Operator switch
            {
                "+" when left is double l && right is double r => l + r,
                "-" when left is double l && right is double r => l - r,
                "*" when left is double l && right is double r => l * r,
                "/" when left is double l && right is double r => l / r,
                "%" when left is double l && right is double r => l % r,

                "+" when left is string || right is string => left.ToString() + right,
                "-" when left is string                    => left.ToString().Replace(right.ToString(), ""),

                "and" when left is bool l && right is bool r   => l && r,
                "or" when left is bool l && right is bool r    => l || r,
                "=" when left is double l && right is double r => Math.Abs(l - r) < LanguageSpecification.Epsilon,
                "=" when left.GetType() == right.GetType()     => left.Equals(right),
                ">" when left is double l && right is double r => l > r,
                "<" when left is double l && right is double r => l < r,

                "+"               => throw LanguageException.RuntimeError($"Cannot add {left} and {right}"),
                "-"               => throw LanguageException.RuntimeError($"Cannot subtract {left} and {right}"),
                "*"               => throw LanguageException.RuntimeError($"Cannot multiply {left} and {right}"),
                "/"               => throw LanguageException.RuntimeError($"Cannot divide {left} and {right}"),
                "%"               => throw LanguageException.RuntimeError($"Cannot modulo {left} and {right}"),
                "and"             => throw LanguageException.RuntimeError($"Cannot and {left}, {right}"),
                "or"              => throw LanguageException.RuntimeError($"Cannot or {left}, {right}"),
                "=" or "<" or ">" => throw LanguageException.RuntimeError($"Cannot compare {left} and {right}"),
                _                 => throw LanguageException.RuntimeError($"Unknown operator {node.Operator}")
            };
        _stack.Push(res);
    }

    public void Visit(Ast.Nodes.UnaryOperator node)
    {
        node.Expression.Accept(this);
        var value = _stack.Pop();
        
        object res = node.Operator switch
        {
            "-" when value is double d => -d,
            "not" when value is bool b => !b,
            
            "-" => throw LanguageException.RuntimeError($"Cannot subtract (-) {value}"),
            "not" => throw LanguageException.RuntimeError($"Cannot negate (not) {value}"),
            _ => throw LanguageException.RuntimeError($"Unknown operator {node.Operator}")
        };
        _stack.Push(res);
    }

    public void Visit(Ast.Nodes.Assignment node)
    {
        node.Expression.Accept(this);
        _variables[node.Name] = _stack.Pop();
    }

    public void Visit(Ast.Nodes.FunctionCall node)
    {
        List<object> args = [];
        foreach (var argument in node.Arguments)
        {
            argument.Accept(this);
            args.Add(_stack.Pop());
        }
        
        if (_functions.TryGetValue(node.Name, out var func))
        {
            object res = func(args);
            if (res is not null)
                _stack.Push(res);
        }
        else
        {
            throw LanguageException.RuntimeError($"Function {node.Name} is not defined");
        }
    }

    public void Visit(Ast.Nodes.Conditional node)
    {
        node.Condition.Accept(this);
        var value = _stack.Pop();
        if (value is not bool b)
            throw LanguageException.RuntimeError($"Cannot evaluate {value} as a boolean");

        if (b)
            node.TrueBranch.Accept(this);
        else
            node.FalseBranch?.Accept(this);
    }

    public void Visit(Ast.Nodes.Compound node)
    {
        foreach (Ast.INode statement in node.Statements)
        {
            statement.Accept(this);
        }
    }
}