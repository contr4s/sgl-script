﻿using System.Globalization;

namespace Sgl_script;

public class Parser(Lexer lexer, ExecutionContext context)
{
    private Token _currentToken = lexer.PopNext();

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
                        TokenType.Identifier => IdentifierAction(),
                        TokenType.Keyword    => Keyword(),
                        _                    => throw LanguageException.SyntaxError(lexer.Line, $"unexpected statement {_currentToken.Value}")
                    };
                statements.Add(statement);
            }
        }
        
        Consume(endToken);
        return new Ast.Nodes.Compound(statements);
    }

    private Ast.INode IdentifierAction()
    {
        string identifier = _currentToken.Value;
        Consume(TokenType.Identifier);

        if (context.HasFunction(identifier))
            return new Ast.Nodes.FunctionCall(identifier, GetArguments());
        
        return _currentToken.Type switch
            {
                TokenType.Equals => Assignment(identifier),
                TokenType.Dot    => Method(new Ast.Nodes.Variable(identifier)),
                _                => throw LanguageException.SyntaxError(lexer.Line, $"unexpected statement {_currentToken.Value}")
            };
    }
    
    private Ast.Nodes.Assignment Assignment(string variable)
    {
        Consume(TokenType.Equals);
        
        string identifier = _currentToken.Value;
        Ast.INode value;
        if (context.HasFunction(identifier))
        {
            Consume(TokenType.Identifier);
            value = new Ast.Nodes.FunctionCall(identifier, GetArguments());
        }
        else if (lexer.PeekNext().Type == TokenType.Dot)
        {
            Consume(TokenType.Identifier);
            value = Method(new Ast.Nodes.Variable(identifier));
        }
        else
        {
            value = Expression();
        }
        return new Ast.Nodes.Assignment(variable, value);
    }
    
    private Ast.INode Keyword()
    {
        var keyword = _currentToken.Value;
        Consume(TokenType.Keyword);

        return keyword switch
            {
                "if" => Conditional(),
                _    => throw LanguageException.SyntaxError(lexer.Line, $"Unexpected keyword {_currentToken.Value}")
            };
    }
    
    private List<Ast.INode> GetArguments()
    {
        var arguments = new List<Ast.INode>();
        while (true)
        {
            if (_currentToken.Type is TokenType.NewLine or TokenType.EndOfFile)
            {
                Consume(_currentToken.Type);
                break;
            }

            arguments.Add(Expression());
            if (_currentToken.Type is not (TokenType.NewLine or TokenType.EndOfFile))
                Consume(TokenType.Comma);
        }

        return arguments;
    }

    private Ast.Nodes.MethodCall Method(Ast.Nodes.Variable variable)
    {
        Consume(TokenType.Dot);
        string methodName = _currentToken.Value;
        Consume(TokenType.Identifier);
        
        if (!context.HasMethod(methodName))
            throw LanguageException.SyntaxError(lexer.Line, $"Unexpected method {methodName}");
        
        return new Ast.Nodes.MethodCall(variable, methodName, GetArguments());
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
            
            case TokenType.OpenBracket:
            {
                Consume(TokenType.OpenBracket);
                var elements = new List<Ast.INode>();
                while (true)
                {
                    if (_currentToken.Type == TokenType.CloseBracket)
                    {
                        Consume(TokenType.CloseBracket);
                        return new Ast.Nodes.Array(elements);
                    }
                    
                    elements.Add(Expression());
                    if (_currentToken.Type != TokenType.CloseBracket)
                        Consume(TokenType.Comma);
                }
            }

            case TokenType.Range:
            {
                var value = _currentToken.Value;
                Consume(TokenType.Range);

                string[] split = value.Split('.', StringSplitOptions.RemoveEmptyEntries);
                if (split.Length < 1 || !int.TryParse(split[0], out int start))
                    throw LanguageException.SyntaxError(lexer.Line, $"Invalid range start {value}");
                if (split.Length < 2 || !int.TryParse(split[1], out int end))
                    throw LanguageException.SyntaxError(lexer.Line, $"Invalid range end {value}");

                IEnumerable<int> range = start <= end
                                             ? Enumerable.Range(start, end - start + 1)
                                             : Enumerable.Range(end, start - end + 1).Reverse();

                return new Ast.Nodes.Array(range.Select(x => new Ast.Nodes.Literal<double>(x)).ToList());
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
            _currentToken = lexer.PopNext();
        else
            throw LanguageException.SyntaxError(lexer.Line, $"Unexpected token {_currentToken.Value}");
    }
}
