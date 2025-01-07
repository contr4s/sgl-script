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

    public static HashSet<string> Keywords { get; } = ["do", "if", "else", "for"];

    public static Dictionary<string, Func<List<object>, object>> StandardFunctions { get; } = new()
        {
            ["print"] = args =>
            {
                Console.WriteLine(string.Join(" ", args.Select(x => x.ToString())));
                return null!;
            }
        };
}