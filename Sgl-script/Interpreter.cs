using System.Globalization;

namespace Sgl_script;

public class Interpreter(ExecutionContext context) : Ast.IVisitor
{
    MemoryManager _memoryManager = new MemoryManager();
    
    private Stack<object> _values = new();
    
    private bool _break;
    private bool _return; 

    public void Run(Ast ast)
    {
        ast.Root.Accept(this);
    }

    public void Visit<T>(Ast.Nodes.Literal<T> node)
    {
        if (node.Value is not null)
            _values.Push(node.Value);
    }

    public void Visit(Ast.Nodes.Array node)
    {
        var array = new List<object>();
        foreach (var el in node.Elements)
            el.Accept(this);

        for (int i = 0; i < node.Elements.Count; i++)
        {
            array.Add(_values.Pop());
        }
        array.Reverse();
        _values.Push(array);
    }

    public void Visit(Ast.Nodes.Variable node)
    {
        _values.Push(_memoryManager.GetValue(node.Name));
    }

    public void Visit(Ast.Nodes.BinaryOperator node)
    {
        node.Left.Accept(this);
        var left = _values.Pop();
        node.Right.Accept(this);
        var right = _values.Pop();

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
        _values.Push(res);
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
        var value = _values.Pop();
        
        object res = node.Operator switch
        {
            "-" when value is double d => -d,
            "not" when value is bool b => !b,
            
            "-" => throw LanguageException.RuntimeError($"Cannot subtract (-) {value}"),
            "not" => throw LanguageException.RuntimeError($"Cannot negate (not) {value}"),
            _ => throw LanguageException.RuntimeError($"Unknown operator {node.Operator}")
        };
        _values.Push(res);
    }

    public void Visit(Ast.Nodes.Assignment node)
    {
        node.Expression.Accept(this);
        _memoryManager.Allocate(node.Variable, _values.Pop());
    }

    public void Visit(Ast.Nodes.FunctionCall node)
    {
        if (!context.HasFunction(node.Name))
            throw LanguageException.RuntimeError($"Function {node.Name} is not defined");
        
        object? res = context.ExecuteFunction(node.Name, PrepareArguments(node.Arguments));
        if (res is not null)
            _values.Push(res);
    }

    private List<object> PrepareArguments(IReadOnlyList<Ast.INode> arguments)
    {
        List<object> args = [];
        foreach (var argument in arguments)
        {
            argument.Accept(this);
            args.Add(_values.Pop());
        }

        return args;
    }

    public void Visit(Ast.Nodes.MethodCall node)
    {
        if (!context.HasMethod(node.Name))
            throw LanguageException.RuntimeError($"Method {node.Name} is not defined");
        
        node.Variable.Accept(this);
        object? res = context.ExecuteMethod(_values.Pop(), node.Name, PrepareArguments(node.Arguments));
        if (res is not null)
            _values.Push(res);
    }

    public void Visit(Ast.Nodes.Conditional node)
    {
        node.Condition.Accept(this);
        var value = _values.Pop();
        if (value is not bool b)
            throw LanguageException.RuntimeError($"Cannot evaluate {value} as a boolean");

        if (b)
            node.TrueBranch.Accept(this);
        else
            node.FalseBranch?.Accept(this);
    }

    public void Visit(Ast.Nodes.Compound node)
    {
        if (node.CreateScope)
            _memoryManager.EnterScope();
        
        foreach (Ast.INode statement in node.Statements)
        {
            if (_return || _break)
                return;
            
            statement.Accept(this);
        }
        
        if (node.CreateScope)
            _memoryManager.ExitScope();
    }

    public void Visit(Ast.Nodes.Loop node)
    {
        node.Iterable.Accept(this);
        var iterable = _values.Pop();
        if (iterable is not List<object> list)
            throw LanguageException.RuntimeError($"Cannot iterate over {iterable}");

        foreach (var el in list)
        {
            _values.Push(el);
            _memoryManager.Allocate(node.IteratorName, _values.Pop());
            
            foreach (Ast.INode bodyStatement in node.Body.Statements)
            {
                if (_break)
                {
                    _break = false;
                    return;
                }
                
                bodyStatement.Accept(this);
            }
        }
    }

    public void Visit(Ast.Nodes.Range node)
    {
        node.Start.Accept(this);
        object first = _values.Pop();
        if (first is not IConvertible startConvertible)
            throw LanguageException.RuntimeError($"Range start {first} is not a number");

        node.End.Accept(this);
        object second = _values.Pop();
        if (second is not IConvertible endConvertible)
            throw LanguageException.RuntimeError($"Range end {second} is not a number");

        int start = startConvertible.ToInt32(CultureInfo.InvariantCulture);
        int end = endConvertible.ToInt32(CultureInfo.InvariantCulture);
        IEnumerable<int> range = start <= end ? Enumerable.Range(start, end - start + 1)
                                     : Enumerable.Range(end, start - end + 1).Reverse();

        _values.Push(range.Select(x => (double)x).Cast<object>().ToList());
    }

    public void Visit(Ast.Nodes.ExecutionFlag node)
    {
        if (node.Flag == "break")
            _break = true;

        if (node.Flag == "return")
            _return = true;
    }
}