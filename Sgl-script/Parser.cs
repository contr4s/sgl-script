using System.Globalization;

namespace Sgl_script;

public class Parser(Lexer lexer)
{
    private Token _currentToken = lexer.NextToken();

    public Ast Parse()
    {
        List<Ast.INode> statements = [];
        while (_currentToken.Type != TokenType.EndOfFile)
        {
            var statement = _currentToken.Type switch 
                    {
                            TokenType.Identifier => Assignment(),
                            TokenType.Keyword    => Keyword(),
                            _ => throw LanguageException.SyntaxError(lexer.Line, $"unexpected statement {_currentToken.Value}")
                    };
            
            statements.Add(statement);
        }
        
        return new Ast(statements);
    }
    
    private Ast.Nodes.Assignment Assignment()
    {
        string variableName = _currentToken.Value;
        Consume(TokenType.Identifier);
        Consume(TokenType.Equals);
        
        var value = Expression();
        
        return new Ast.Nodes.Assignment(variableName, value);
    }
    
    private Ast.INode Keyword()
    {
        var keyword = _currentToken.Value;
        Consume(TokenType.Keyword);

        return keyword switch
            {
                "do" => Function(),
                _    => throw LanguageException.SyntaxError(lexer.Line, $"Unexpected keyword {_currentToken.Value}")
            };
    }

    private Ast.Nodes.FunctionCall Function()
    {
        string functionName = _currentToken.Value;
        Consume(TokenType.Identifier);
        
        var arguments = new List<Ast.INode>();
        while (true)
        {
            arguments.Add(Expression());
            if (_currentToken.Type == TokenType.CloseParenthesis)
            {
                Consume(TokenType.CloseParenthesis);
                break;
            }

            Consume(TokenType.Comma);
        }
        
        return new Ast.Nodes.FunctionCall(functionName, arguments);
    }

    private Ast.INode Expression()
    {
        var left = Term();

        while (_currentToken.Type == TokenType.BinaryOperator1)
        {
            string operatorSymbol = _currentToken.Value;
            Consume(_currentToken.Type);
            var right = Term();
            left = new Ast.Nodes.BinaryOperator(left, operatorSymbol, right);
        }

        return left;
    }

    private Ast.INode Term()
    {
        var node = Factor();

        while (_currentToken.Type == TokenType.BinaryOperator2)
        {
            string operatorSymbol = _currentToken.Value;
            Consume(_currentToken.Type);
            var right = Factor();
            node = new Ast.Nodes.BinaryOperator(node, operatorSymbol, right);
        }

        return node;
    }

    private Ast.INode Factor() 
    {
        switch (_currentToken.Type)
        {
            case TokenType.Number:
            {
                var value = double.Parse(_currentToken.Value, CultureInfo.InvariantCulture);
                Consume(TokenType.Number);
                return new Ast.Nodes.Literal<double>(value);
            }

            case TokenType.String:
            {
                var value = _currentToken.Value;
                Consume(TokenType.String);
                return new Ast.Nodes.Literal<string>(value);
            }

            case TokenType.Identifier:
            {
                var name = _currentToken.Value;
                Consume(TokenType.Identifier);
                return new Ast.Nodes.Variable(name);
            }

            case TokenType.OpenParenthesis:
            {
                Consume(TokenType.OpenParenthesis);
                var node = Expression();
                Consume(TokenType.CloseParenthesis);
                return node;
            }

            default:
                throw LanguageException.SyntaxError(lexer.Line,$"Unexpected token {_currentToken.Value}");
        }
    }

    private void Consume(TokenType type)
    {
        if (_currentToken.Type == type)
            _currentToken = lexer.NextToken();
        else
            throw LanguageException.SyntaxError(lexer.Line, $"Unexpected token {_currentToken.Value}");
    }
}
