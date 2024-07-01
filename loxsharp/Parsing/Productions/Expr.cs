using loxsharp.Scanning;

namespace loxsharp.Parsing.Productions;

public abstract record Expr
{
    public abstract T Accept<T>(ISyntaxTreeVisitor<T> visitor);
}

public record Assign(Token Token, Expr Value) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitAssign(this);
   }
}

public record Binary(Expr Left, Token Token, Expr Right) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitBinary(this);
   }
}

public record Call(Expr Callee, Token Paren, List<Expr> Arguments) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitCall(this);
   }
}

public record Get(Expr Object, Token Token) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitGet(this);
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

public record Logical(Expr Left, Token Operator, Expr Right) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitLogical(this);
   }
}

public record Set(Expr Object, Token Token, Expr Value) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitSet(this);
   }
}

public record Unary(Token Token, Expr Right) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitUnary(this);
   }
}

public record Conditional(Expr Condition, Expr ExprIfTrue, Expr ExprIfFalse) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitConditional(this);
   }
}

public record Variable(Token Token) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitVariable(this);
   }
}

public record Lambda(Token Token, List<Token> Params, List<Stmt> Statements) : Expr
{
   public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
   {
      return visitor.VisitLambda(this);
   }
}

