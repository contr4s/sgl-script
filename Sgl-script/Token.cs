namespace Sgl_script;

public record Token(TokenType Type, string Value);

public enum TokenType
{
    Number,
    String,
    Identifier,
    Equals,
    Minus,
    BinaryOperator,
    UnaryOperator,
    OpenParenthesis,
    CloseParenthesis,
    OpenBracket,
    CloseBracket,
    CloseBrace,
    Comma,
    Dot,
    EndOfFile,
    NewLine,
    Keyword,
}