namespace Sgl_script;

public class Ast(IReadOnlyList<Ast.INode> statements)
{
    public IReadOnlyList<INode> Statements { get; } = statements;

    public interface INode
    {
        void Accept(IVisitor visitor);
    }
    
    public interface IVisitor
    {
        void Visit<T>(Nodes.Literal<T> node);
        void Visit(Nodes.Variable node);
        void Visit(Nodes.BinaryOperator node);
        void Visit(Nodes.Assignment node);
        void Visit(Nodes.FunctionCall node);
    }
    
    public static class Nodes
    {
        public record Literal<T>(T Value) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }

        public record Variable(string Name) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }

        public record BinaryOperator(INode Left, string Operator, INode Right) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }

        public record Assignment(string Name, INode Expression) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }

        public record FunctionCall(string Name, IReadOnlyList<INode> Arguments) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }
    }
}