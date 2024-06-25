using loxsharp.Parsing.Productions;

namespace loxsharp.Parsing;

public interface IStatementVisitor<out T>
{
	T VisitExpression(Expression expression);

	T VisitPrint(Print print);

	T VisitVar(Var var);

	T VisitBlock(Block block);

	T VisitIf(If ifStmt);

	T VisitWhile(While whileStmt);

	T VisitFor(For forStmt);
}