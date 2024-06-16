using loxsharp.Parsing.Productions;
using loxsharp.Scanning;

namespace loxsharp.Parsing;

public class Parser
{
	private readonly List<Token> _tokens;
	private int _current = 0;

	private readonly Action<int, string> _reportError;

	public Parser(List<Token> tokens, Action<int,string> reportError)
	{
		_tokens = tokens;
		_reportError = reportError;
	}

	public Expr Parse()
	{
		try
		{
			return Expression();
		}
		catch (ParseException exception)
		{
			return null;
		}
	}

	private Expr Expression()
	{
		return Equality();
	}

	private Expr Equality()
	{
		var expr = Comparison();

		while (Match(TokenType.BANG_EQUAL,TokenType.EQUAL_EQUAL))
		{
			expr = new Binary(expr, Previous(), Comparison());
		}

		return expr;
	}

	private Expr Comparison()
	{
		var expr = Term();

		while (Match(TokenType.GREATER,
			       TokenType.GREATER_EQUAL,
			       TokenType.LESS,
			       TokenType.LESS_EQUAL))
		{
			expr = new Binary(expr, Previous(), Term());
		}

		return expr;
	}

	private Expr Term()
	{
		var expr = Factor();

		while (Match(TokenType.MINUS,
			       TokenType.PLUS))
		{
			expr = new Binary(expr, Previous(), Factor());
		}

		return expr;
	}

	private Expr Factor()
	{
		var expr = Unary();

		while (Match(TokenType.STAR,
			       TokenType.SLASH))
		{
			expr = new Binary(expr, Previous(), Unary());
		}

		return expr;
	}

	private Expr Unary()
	{
		return Match(TokenType.BANG, TokenType.MINUS)
			? new Unary(Previous(), Unary())
			: Primary();
	}

	private Expr Primary()
	{
		if (Match(TokenType.FALSE)) return new Literal(false);
		if (Match(TokenType.TRUE)) return new Literal(true);
		if (Match(TokenType.NIL)) return new Literal(null);

		if (Match(TokenType.NUMBER, TokenType.STRING)) {
			return new Literal(Previous().Literal);
		}

		if (Match(TokenType.LEFT_PAREN)) {
			var expr = Expression();
			Consume(TokenType.RIGHT_PAREN, "Expect ')' after an expression.");
			return new Grouping(expr);
		}

		throw Error(Peek(), "Expression is expected.");
	}

	// checks if token has one of given types
	private bool Match(params TokenType[] types)
	{
		if (types.All(x => Peek().Type != x)) return false;
		Advance();
		return true;
	}

	private Token Consume(TokenType type, string message)
	{
		if (Peek().Type == type) return Advance();

		throw Error(Peek(), message);
	}

	private ParseException Error(Token token, string message)
	{
		var position = token.Type == TokenType.EOF ? "end" : token.Lexeme;
		_reportError(token.Line, $"At {position}. {message}");
		return new ParseException();
	}

	private void Synchronize()
	{
		Advance();

		while (!IsAtEnd())
		{
			if (Previous().Type == TokenType.SEMICOLON) return;

			if (Peek().Type is TokenType.CLASS
			    or TokenType.FUN
			    or TokenType.VAR
			    or TokenType.FOR
			    or TokenType.IF
			    or TokenType.WHILE
			    or TokenType.PRINT
			    or TokenType.RETURN)
				return;

			Advance();
		}
	}

	private bool IsAtEnd()
	{
		return Peek().Type == TokenType.EOF;
	}

	private Token Peek()
	{
		return _tokens[_current];
	}

	private Token Previous()
	{
		return _tokens[_current - 1];
	}

	private Token Advance()
	{
		if (!IsAtEnd()) _current++;
		return Previous();
	}
}