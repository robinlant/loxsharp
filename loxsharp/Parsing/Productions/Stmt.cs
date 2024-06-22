using loxsharp.Scanning;

namespace loxsharp.Parsing.Productions;

public abstract record Stmt
{
	public abstract T Accept<T>(IStatementVisitor<T> visitor);
}

public record Expression(Expr Expr) : Stmt
{
	public override T Accept<T>(IStatementVisitor<T> visitor)
	{
		return visitor.VisitExpression(this);
	}
}

public record Print(Expr Expr) : Stmt
{
	public override T Accept<T>(IStatementVisitor<T> visitor)
	{
		return visitor.VisitPrint(this);
	}
}

public record Var(Token Token, Expr? Init) : Stmt
{
	public override T Accept<T>(IStatementVisitor<T> visitor)
	{
		return visitor.VisitVar(this);
	}
}