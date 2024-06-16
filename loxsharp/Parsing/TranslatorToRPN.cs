using System.Text;
using loxsharp.Parsing.Productions;

namespace loxsharp.Parsing;

public class TranslatorToRPN : ISyntaxTreeVisitor<string>
{
	public string VisitBinary(Binary binary)
	{
		var strBuilder = new StringBuilder()
			.Append(binary.Left.Accept(this))
			.Append(' ')
			.Append(binary.Right.Accept(this))
			.Append(' ')
			.Append(binary.Token.Lexeme);

		return strBuilder.ToString();
	}

	public string VisitGrouping(Grouping grouping)
	{
		return grouping.Expression.Accept(this);
	}

	public string VisitLiteral(Literal literal)
	{
		return literal.Value.ToString();
	}

	public string VisitUnary(Unary unary)
	{
		var strBuilder = new StringBuilder()
			.Append(unary.Right.Accept(this))
			.Append(' ')
			.Append(unary.Token.Lexeme)
			.Append(' ')
			.Append("UNARY");

		return strBuilder.ToString();
	}
}