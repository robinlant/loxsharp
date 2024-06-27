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

public record Block(List<Stmt> Stmts) : Stmt
{
	public override T Accept<T>(IStatementVisitor<T> visitor)
	{
		return visitor.VisitBlock(this);
	}
}

public record If(Expr Condition, Stmt ThenStmt, Stmt? ElseStmt = null) : Stmt
{
	public override T Accept<T>(IStatementVisitor<T> visitor)
	{
		return visitor.VisitIf(this);
	}
}

public record While(Expr Condition, Stmt ThenStmt) : Stmt
{
	public override T Accept<T>(IStatementVisitor<T> visitor)
	{
		return visitor.VisitWhile(this);
	}
}

public record For(Stmt? Init, Expr? Condition, Expr? Increment, Stmt Stmt) : Stmt
{
	public override T Accept<T>(IStatementVisitor<T> visitor)
	{
		return visitor.VisitFor(this);
	}
}

public record Break(Token Token) : Stmt
{
	public override T Accept<T>(IStatementVisitor<T> visitor)
	{
		return visitor.VisitBreak(this);
	}
}

public record Function(Token Name, List<Token> Params, List<Stmt> Body) : Stmt
{
	public override T Accept<T>(IStatementVisitor<T> visitor)
	{
		return visitor.VisitFunction(this);
	}
}

public record Return(Token Token, Expr? Value) : Stmt
{
	public override T Accept<T>(IStatementVisitor<T> visitor)
	{
		return visitor.VisitReturn(this);
	}
}