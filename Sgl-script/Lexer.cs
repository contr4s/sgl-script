namespace Sgl_script;

public class Lexer(string input)
{
    private int _position;
    public int Line { get; private set; } = 1;

    private string PeekWhile(Predicate<char> predicate, bool throwOnError = false)
    {
        int start = _position;
        while (predicate(input[_position]))
        {
            _position++;
            
            if (_position >= input.Length)
            {
                if (throwOnError)
                    throw LanguageException.SyntaxError(Line, $"Unexpected end of input in literal {input[start.._position]}");
                
                break;
            }
        }
        
        return input[start.._position];
    }
    
    public Token NextToken()
    {
        if (_position >= input.Length)
            return new Token(TokenType.EndOfFile, "");
        
        PeekWhile(c => c is ' ' or '\t' or '\r');
        
        if (_position >= input.Length)
            return new Token(TokenType.EndOfFile, "");
        
        char currentChar = input[_position];

        if (currentChar is '\n')
        {
            Line++;
            _position++;
            return new Token(TokenType.NewLine, "");
        }
        
        if (char.IsDigit(currentChar))
            return new Token(TokenType.Number, PeekWhile(c => char.IsDigit(c) || c == '.'));

        if (currentChar == '"')
        {
            _position++;
            string literal = PeekWhile(c => c != '"', true);
            _position++;
            return new Token(TokenType.String, literal);
        }

        if (char.IsLetter(currentChar))
        {
            string literal = PeekWhile(char.IsLetterOrDigit);
            if (LanguageSpecification.Keywords.Contains(literal))
                return new Token(TokenType.Keyword, literal);
            
            if (LanguageSpecification.OperatorKeywords.TryGetValue(literal, out var type))
                return new Token(type, literal);
            
            return new Token(TokenType.Identifier, literal);
        }

        if (LanguageSpecification.SpecialTokens.TryGetValue(currentChar, out var tokenType))
        {
            _position++;
            return new Token(tokenType, currentChar.ToString());
        }

        throw LanguageException.SyntaxError(Line, $"Unknown symbol: {currentChar}");
    }
}
