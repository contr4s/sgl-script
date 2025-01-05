namespace Sgl_script;

public enum TokenType
{
    Number,
    String,
    Identifier,
    Equals,
    BinaryOperator1,
    BinaryOperator2,
    OpenParenthesis,
    CloseParenthesis,
    OpenBracket,
    CloseBracket,
    CloseBrace,
    Comma,
    EndOfFile,
    Keyword,
}

public record Token(TokenType Type, string Value);

public class Lexer(string input)
{
    private static readonly Dictionary<char, TokenType> _specialTokens = new Dictionary<char, TokenType>
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

    private static readonly HashSet<string> _keywords = ["do", "if", "else", "for"];

    private int _position;

    public int Line { get; private set; } = 1;

    private string PeekWhile(Predicate<char> predicate, bool throwOnError)
    {
        int start = _position;
        while (predicate(input[_position]))
        {
            if (input[_position] == '\n')
                Line++;
            
            _position++;
            
            if (_position >= input.Length)
            {
                if (throwOnError)
                    throw new Exception($"Syntax error at line {Line}: Unexpected end of input in literal {input[start.._position]}");
                
                break;
            }
        }
        
        return input[start.._position];
    }
    
    public Token NextToken()
    {
        if (_position >= input.Length)
            return new Token(TokenType.EndOfFile, "");
        
        PeekWhile(char.IsWhiteSpace, true);
        
        if (_position >= input.Length)
            return new Token(TokenType.EndOfFile, "");
        
        char currentChar = input[_position];

        if (char.IsDigit(currentChar) || currentChar == '.')
            return new Token(TokenType.Number, PeekWhile(c => char.IsDigit(c) || c == '.', false));

        if (currentChar == '"')
        {
            _position++;
            string literal = PeekWhile(c => c != '"', true);
            _position++;
            return new Token(TokenType.String, literal);
        }

        if (char.IsLetter(currentChar))
        {
            string literal = PeekWhile(char.IsLetterOrDigit, false);
            return new Token(_keywords.Contains(literal) ? TokenType.Keyword : TokenType.Identifier, literal);
        }

        if (_specialTokens.TryGetValue(currentChar, out var tokenType))
        {
            _position++;
            return new Token(tokenType, currentChar.ToString());
        }

        throw new Exception($"Неизвестный символ: {currentChar}");
    }
}
