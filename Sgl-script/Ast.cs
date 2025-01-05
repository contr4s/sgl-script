namespace Sgl_script;

public class Ast(IReadOnlyList<Ast.INode> statements)
{
    public IReadOnlyList<INode> Statements { get; } = statements;

    public interface INode;
    
    public static class Nodes
    {
        public record Literal<T>(T Value) : INode;
        
        public record Variable(string Name) : INode;
        
        public record BinaryOperator(INode Left, string Operator, INode Right) : INode;
        
        public record Assignment(string Name, INode Value) : INode;
        
        public record FunctionCall(string Name, IReadOnlyList<INode> Arguments) : INode;
    }
}