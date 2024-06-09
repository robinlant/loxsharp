using System.Text;
using loxsharp.Parser.Productions;

namespace loxsharp.Parser;

public class AstPrinter : ISyntaxTreeVisitor<String>
{
	public string VisitBinary(Binary binary)
	{
		return Parenthesize(binary.Token.Lexeme, binary.Left, binary.Right);
	}


	public string VisitGrouping(Grouping grouping)
	{
		return Parenthesize("group", grouping.Expression);
	}

	public string VisitLiteral(Literal literal)
	{
		if (literal.Value is null) return "nil";

		return literal.Value.ToString();

	}

	public string VisitUnary(Unary unary)
	{
		return Parenthesize(unary.Token.Lexeme, unary.Right);
	}

	private string Parenthesize(string name, params Expr[] exprs)
	{
		var strBuilder = new StringBuilder();

		strBuilder.Append('(')
			.Append(name);

		foreach (var i in exprs)
		{
			strBuilder.Append(' ')
				.Append(i.Accept(this));
		}

		return strBuilder.Append(')').ToString();
	}
}