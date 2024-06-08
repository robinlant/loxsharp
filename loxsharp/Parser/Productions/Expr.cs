using loxsharp.Scanner;

namespace loxsharp.Parser.Productions;

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

public record Grouping(Expr Left, Token Token, Expr Right) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitGrouping(this);
   }
}

public record Literal(Expr Left, Token Token, Expr Right) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitLiteral(this);
   }
}

public record Unary(Expr Left, Token Token, Expr Right) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitUnary(this);
   }
}

