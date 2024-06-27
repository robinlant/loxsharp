using System.Linq.Expressions;
using System.Text;
using loxsharp.Parsing.Productions;

namespace loxsharp.Parsing;

public class AstPrinter : ISyntaxTreeVisitor<string>
{
	public string VisitAssign(Assign assign)
	{
		throw new NotImplementedException();
	}

	public string VisitBinary(Binary binary)
	{
		var strBuilder = new StringBuilder("( ")
			.Append(binary.Left.Accept(this))
			.Append(' ')
			.Append(binary.Token.Lexeme)
			.Append(' ')
			.Append(binary.Right.Accept(this))
			.Append(" )");

		return strBuilder.ToString();
	}

	public string VisitCall(Call call)
	{
		throw new NotImplementedException();
	}

	public string VisitGrouping(Grouping grouping)
	{
		return Parenthesize("group", grouping.Expression);
	}

	public string VisitLiteral(Literal literal)
	{
		if (literal.Value is null) return "nil";

		return literal.Value.ToString() ?? string.Empty;
	}

	public string VisitLogical(Logical logical)
	{
		throw new NotImplementedException();
	}

	public string VisitUnary(Unary unary)
	{
		return Parenthesize(unary.Token.Lexeme, unary.Right);
	}

	public string VisitConditional(Conditional conditional)
	{
		var strBuilder = new StringBuilder("( ")
			.Append("ternary ")
			.Append(Parenthesize("condition", conditional.Condition))
			.Append(" ? ")
			.Append(Parenthesize("", conditional.ExprIfFalse))
			.Append(" : ")
			.Append(Parenthesize("", conditional.ExprIfFalse))
			.Append(" )");

		return strBuilder.ToString();
	}

	public string VisitVariable(Variable variable)
	{
		throw new NotImplementedException();
	}

	private string Parenthesize(string name, params Expr[] exprs)
	{
		var strBuilder = new StringBuilder();

		strBuilder.Append("( ")
			.Append(name);

		foreach (var i in exprs)
		{
			strBuilder.Append(' ')
				.Append(i.Accept(this));
		}

		return strBuilder.Append(" )").ToString();
	}
}