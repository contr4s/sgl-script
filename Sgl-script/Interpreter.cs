namespace Sgl_script;

public class Interpreter(ExecutionContext context) : Ast.IVisitor
{
    private Dictionary<string, object> _variables = new();
    
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

    public void Visit(Ast.Nodes.Array node)
    {
        var array = new List<object>();
        foreach (var el in node.Elements)
            el.Accept(this);

        for (int i = 0; i < node.Elements.Count; i++)
        {
            array.Add(_stack.Pop());
        }
        array.Reverse();
        _stack.Push(array);
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

                "+" when left is List<object> l && right is List<object> r => Add(l, r),
                "-" when left is List<object> l && right is List<object> r => Subtract(l, r),
                "+" when left is List<object> l                         => Add(l, right),
                "-" when left is List<object> l                         => Subtract(l, right),
                
                "+" when left is string || right is string => left.ToString() + right,
                "-" when left is string => left.ToString().Replace(right.ToString(), ""),

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
    
    private List<object> Add(List<object> left, object right)
    {
        var clone = left.ToList();
        clone.Add(right);
        return clone;
    }
    
    private List<object> Add(List<object> left, List<object> right)
    {
        var clone = left.ToList();
        clone.AddRange(right);
        return clone;
    }
    
    private List<object> Subtract(List<object> left, object right)
    {
        var clone = left.ToList();
        clone.Remove(right);
        return clone;
    }
    
    private List<object> Subtract(List<object> left, List<object> right)
    {
        var clone = left.ToList();
        foreach (object el in right)
        {
            clone.Remove(el);
        }
        return clone;
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
        _variables[node.Variable] = _stack.Pop();
    }

    public void Visit(Ast.Nodes.FunctionCall node)
    {
        if (!context.HasFunction(node.Name))
            throw LanguageException.RuntimeError($"Function {node.Name} is not defined");
        
        object? res = context.ExecuteFunction(node.Name, PrepareArguments(node.Arguments));
        if (res is not null)
            _stack.Push(res);
    }

    private List<object> PrepareArguments(IReadOnlyList<Ast.INode> arguments)
    {
        List<object> args = [];
        foreach (var argument in arguments)
        {
            argument.Accept(this);
            args.Add(_stack.Pop());
        }

        return args;
    }

    public void Visit(Ast.Nodes.MethodCall node)
    {
        if (!context.HasMethod(node.Name))
            throw LanguageException.RuntimeError($"Method {node.Name} is not defined");
        
        node.Variable.Accept(this);
        object? res = context.ExecuteMethod(_stack.Pop(), node.Name, PrepareArguments(node.Arguments));
        if (res is not null)
            _stack.Push(res);
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