using System.Globalization;

namespace Sgl_script;

public class Parser(Lexer lexer)
{
    private Token _currentToken = lexer.NextToken();

    public Ast Parse() => new (Compound(TokenType.EndOfFile));
    
    private Ast.Nodes.Compound Compound(TokenType endToken)
    {
        List<Ast.INode> statements = [];
        while (_currentToken.Type != endToken)
        {
            if (_currentToken.Type == TokenType.NewLine)
            {
                Consume(TokenType.NewLine);
            }
            else
            {
                var statement = _currentToken.Type switch
                    {
                        TokenType.Identifier => Assignment(),
                        TokenType.Keyword    => Keyword(),
                        _                    => throw LanguageException.SyntaxError(lexer.Line, $"unexpected statement {_currentToken.Value}")
                    };
                statements.Add(statement);
            }
        }
        
        Consume(endToken);
        return new Ast.Nodes.Compound(statements);
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
                "if" => Conditional(),
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
            if (_currentToken.Type == TokenType.NewLine)
            {
                Consume(TokenType.NewLine);
                break;
            }

            Consume(TokenType.Comma);
        }
        
        return new Ast.Nodes.FunctionCall(functionName, arguments);
    }
    
    private Ast.Nodes.Conditional Conditional()
    {
        var condition = Expression();
        var trueBranch = Compound(TokenType.CloseBrace);

        SkipWhitespaces();
        
        Ast.Nodes.Compound falseBranch = null;
        if (_currentToken is {Type: TokenType.Keyword, Value: "else"})
        {
            Consume(TokenType.Keyword);
            falseBranch = Compound(TokenType.CloseBrace);
        }
        
        return new Ast.Nodes.Conditional(condition, trueBranch, falseBranch);
    }

    private void SkipWhitespaces()
    {
        while (_currentToken.Type is TokenType.NewLine)
        {
            Consume(TokenType.NewLine);
        }
    }

    private Ast.INode Expression(int priority = LanguageSpecification.MaxPriority)
    {
        if (priority <= 0)
            return Factor();

        priority--;
        var expression = Expression(priority);
        
        while (LanguageSpecification.OperatorsPriority.TryGetValue(_currentToken.Value, out int p) && p < priority)
        {
            string operatorSymbol = _currentToken.Value;
            Consume(_currentToken.Type);
            
            var right = Expression(priority);
            expression = new Ast.Nodes.BinaryOperator(expression, operatorSymbol, right);
        }

        return expression;
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
                string value = _currentToken.Value;
                Consume(TokenType.String);
                return new Ast.Nodes.Literal<string>(value);
            }

            case TokenType.Identifier:
            {
                string name = _currentToken.Value;
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

            case TokenType.Minus or TokenType.UnaryOperator:
            {
                string unaryOperator = _currentToken.Value;
                Consume(_currentToken.Type);
                return new Ast.Nodes.UnaryOperator(unaryOperator, Factor());
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
