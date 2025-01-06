namespace Sgl_script;

public static class LanguageSpecification
{
    public static Dictionary<char, TokenType> SpecialTokens { get; } = new Dictionary<char, TokenType>
        {
            ['='] = TokenType.Equals,
            ['('] = TokenType.OpenParenthesis,
            [')'] = TokenType.CloseParenthesis,
            ['['] = TokenType.OpenBracket,
            [']'] = TokenType.CloseBracket,
            ['}'] = TokenType.CloseBrace,
            [','] = TokenType.Comma,
            ['+'] = TokenType.BinaryOperator1,
            ['-'] = TokenType.BinaryOperator1,
            ['*'] = TokenType.BinaryOperator2,
            ['/'] = TokenType.BinaryOperator2,
            ['%'] = TokenType.BinaryOperator2,
        };

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