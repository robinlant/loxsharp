using loxsharp.Scanning;

namespace loxsharp.Parsing.Productions;

public abstract record Expr
{
    public abstract T Accept<T>(ISyntaxTreeVisitor<T> visitor);
}

public record Binary(Expr Left, Token Token, Expr Right) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitBinary(this);
   }
}

public record Grouping(Expr Expression) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitGrouping(this);
   }
}

public record Literal(object? Value) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitLiteral(this);
   }
}

public record Unary(Token Token, Expr Right) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitUnary(this);
   }
}

public record Conditional(Expr Condition, Expr ValueIfTrue, Expr ValueIfFalse) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitConditional(this);
   }
}

