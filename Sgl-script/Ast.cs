using System.Diagnostics.CodeAnalysis;

namespace Sgl_script;

public record Ast(Ast.Nodes.Compound Root)
{
    public interface INode
    {
        void Accept(IVisitor visitor);
    }
    
    public interface IVisitor
    {
        void Visit<T>(Nodes.Literal<T> node);
        void Visit(Nodes.Array node);
        void Visit(Nodes.Variable node);
        void Visit(Nodes.BinaryOperator node);
        void Visit(Nodes.UnaryOperator node);
        void Visit(Nodes.Assignment node);
        void Visit(Nodes.FunctionCall node);
        void Visit(Nodes.MethodCall node);
        void Visit(Nodes.Conditional node);
        void Visit(Nodes.Compound node);
        void Visit(Nodes.Loop node);
        void Visit(Nodes.Range node);
        void Visit(Nodes.ExecutionFlag node);
    }
    
    public static class Nodes
    {
        public record Literal<T>(T Value) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }
        
        public record Array(IReadOnlyList<INode> Elements) : INode
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
        
        public record UnaryOperator(string Operator, INode Expression) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }

        public record Assignment(string Variable, INode Expression) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }

        public record FunctionCall(string Name, IReadOnlyList<INode> Arguments) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }
        
        public record MethodCall(Variable Variable, string Name, IReadOnlyList<INode> Arguments) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }
        
        public record Conditional(INode Condition, Compound TrueBranch, [AllowNull] Compound FalseBranch) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }
        
        public record Compound(IReadOnlyList<INode> Statements) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }
        
        public record Loop(string IteratorName, INode Iterable, Compound Body) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }
        
        public record Range(INode Start, INode End) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }

        public record ExecutionFlag(string Flag) : INode
        {
            public void Accept(IVisitor visitor) => visitor.Visit(this);
        }
    }
}