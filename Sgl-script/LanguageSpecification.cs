using System.Collections;

namespace Sgl_script;

public static class LanguageSpecification
{
    public const double Epsilon = 1e-10;
    
    public static Dictionary<char, TokenType> SpecialTokens { get; } = new Dictionary<char, TokenType>
        {
            ['='] = TokenType.Equals,
            ['('] = TokenType.OpenParenthesis,
            [')'] = TokenType.CloseParenthesis,
            ['['] = TokenType.OpenBracket,
            [']'] = TokenType.CloseBracket,
            ['}'] = TokenType.CloseBrace,
            [','] = TokenType.Comma,
            ['.'] = TokenType.Dot,
            ['-'] = TokenType.Minus,
            ['+'] = TokenType.BinaryOperator,
            ['*'] = TokenType.BinaryOperator,
            ['/'] = TokenType.BinaryOperator,
            ['%'] = TokenType.BinaryOperator,
            ['>'] = TokenType.BinaryOperator,
            ['<'] = TokenType.BinaryOperator,
        };
    
    public static Dictionary<string, TokenType> OperatorKeywords { get; } = new()
    {
        ["not"] = TokenType.UnaryOperator,
        ["and"] = TokenType.BinaryOperator,
        ["or"] = TokenType.BinaryOperator
    };
    
    public static Dictionary<string, int> OperatorsPriority { get; } = new()
    {
        ["not"] = 1,
        ["*"] = 2,
        ["/"] = 2,
        ["%"] = 2,
        ["+"] = 3,
        ["-"] = 3,
        [">"] = 4,
        ["<"] = 4,
        ["="] = 5,
        ["and"] = 6,
        ["or"] = 7,
    };
    public const int MaxPriority = 9;

    public static HashSet<string> Keywords { get; } = ["if", "else", "for"];

    public static Dictionary<string, Func<List<object>, object>> StandardFunctions { get; } = new()
        {
            ["Print"] = args =>
            {
                Console.WriteLine(string.Join(" ", args.Select(NiceToString)));
                return null!;
            }
        };

    private static string NiceToString(object o)
    {
        if (o is IList<object> list)
            return $"[{string.Join(", ", list.Select(NiceToString))}]";

        return o.ToString();
    }
    
    public static Dictionary<string, Func<object, List<object>, object>> StandardMethods { get; } = new()
        {
            ["At"] = (obj, args) =>
            {
                if (obj is not IList list)
                    throw LanguageException.RuntimeError($"Cannot use 'at' method on non-list object {obj}");

                if (args.Count is not 1)
                    throw LanguageException.RuntimeError($"Cannot use 'at' method with {args.Count} arguments");

                if (!int.TryParse(args[0].ToString(), out int index))
                    throw LanguageException.RuntimeError($"Cannot use 'at' method with non-integer argument {args[0]}");
                
                if (index < 0 || index >= list.Count)
                    throw LanguageException.RuntimeError($"Index out of range {index}");

                return list[index];
            },
            ["Count"] = (obj, _) =>
            {
                if (obj is not IList list)
                    throw LanguageException.RuntimeError($"Cannot use 'len' method on non-list object {obj}");
                return list.Count;
            },
            ["Add"] = (obj, args) =>
            {
                if (obj is not List<object> list)
                    throw LanguageException.RuntimeError($"Cannot use 'add' method on non-list object {obj}");
                list.AddRange(args);
                return null!;
            },
            ["Remove"] = (obj, args) =>
            {
                if (obj is not List<object> list)
                    throw LanguageException.RuntimeError($"Cannot use 'remove' method on non-list object {obj}");
                foreach (object el in args)
                {
                    list.Remove(el);
                }
                return null;
            },
            ["Insert"] = (obj, args) =>
            {
                if (obj is not List<object> list)
                    throw LanguageException.RuntimeError($"Cannot use 'insert' method on non-list object {obj}");
                
                if (args.Count is not 2)
                    throw LanguageException.RuntimeError($"Cannot use 'insert' method with {args.Count} arguments");
                
                if (!int.TryParse(args[0].ToString(), out int index))
                    throw LanguageException.RuntimeError($"Cannot use 'insert' method with non-integer argument {args[0]}");
                
                if (index < 0 || index >= list.Count)
                    throw LanguageException.RuntimeError($"Index out of range {index}");
                
                list.Insert(index, args[1]);
                return null;
            },
            ["RemoveAt"] = (obj, args) =>
            {
                if (obj is not List<object> list)
                    throw LanguageException.RuntimeError($"Cannot use 'removeat' method on non-list object {obj}");
                
                if (args.Count is not 1)
                    throw LanguageException.RuntimeError($"Cannot use 'removeat' method with {args.Count} arguments");
                
                if (!int.TryParse(args[0].ToString(), out int index))
                    throw LanguageException.RuntimeError($"Cannot use 'removeat' method with non-integer argument {args[0]}");
                
                if (index < 0 || index >= list.Count)
                    throw LanguageException.RuntimeError($"Index out of range {index}");
                
                list.RemoveAt(index);
                return null;
            },
            ["Clear"] = (obj, _) =>
            {
                if (obj is not List<object> list)
                    throw LanguageException.RuntimeError($"Cannot use 'clear' method on non-list object {obj}");
                list.Clear();
                return null;
            },
            ["Contains"] = (obj, args) =>
            {
                if (obj is not IList list)
                    throw LanguageException.RuntimeError($"Cannot use 'contains' method on non-list object {obj}");
                return args.Any(list.Contains);
            },
            ["ContainsAll"] = (obj, args) =>
            {
                if (obj is not IList list)
                    throw LanguageException.RuntimeError($"Cannot use 'contains' method on non-list object {obj}");
                return args.All(list.Contains);
            },
        };
}