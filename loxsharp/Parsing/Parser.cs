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

	public List<Stmt> Parse()
	{
		var statements = new List<Stmt>();

		while (!IsAtEnd())
		{
			statements.Add(Declaration());
		}

		return statements;
	}

	private Stmt Declaration()
	{
		try
		{
			return Match(TokenType.VAR) ? VarDeclaration() : Statement();
		}
		catch (ParseException e)
		{
			Synchronize();
			return null!;
		}
	}

	private Stmt VarDeclaration()
	{
		var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

		Expr? init = null;
		if (Match(TokenType.EQUAL))
		{
			init = Expression();
		}

		Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
		return new Var(name, init);
	}

	private Stmt Statement()
	{
		if (Match(TokenType.PRINT)) return PrintStatement();

		return ExpressionStatement();
	}

	private Stmt ExpressionStatement()
	{
		var expression = Comma();

		Consume(TokenType.SEMICOLON, "Expect ';' after value.");

		return new Expression(expression);
	}

	private Stmt PrintStatement()
	{
		var expression = Comma();

		Consume(TokenType.SEMICOLON, "Expect ';' after value.");

		return new Print(expression);
	}

	private Expr Expression()
	{
		return Comma();
	}

	private Expr Comma()
	{
		return ParseBinary(Conditional,
			x => new Binary(x, Previous(), Conditional()),
			TokenType.COMMA);
	}

	private Expr Conditional()
	{
		var expr = Equality();

		if (!Match(TokenType.QUESTION)) return expr;

		var exprIfTrue = Conditional();

		Consume(TokenType.COLON, "Expect :");

		var exprIfFalse = Conditional();

		return new Conditional(expr, exprIfTrue, exprIfFalse);
	}

	private Expr Equality()
	{
		return ParseBinary(Comparison,
			x => new Binary(x, Previous(), Comparison()),
			TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL);
	}

	private Expr Comparison()
	{
		return ParseBinary(Term,
			x => new Binary(x, Previous(), Term()),
			TokenType.GREATER,
			TokenType.GREATER_EQUAL,
			TokenType.LESS,
			TokenType.LESS_EQUAL);
	}

	private Expr Term()
	{
		return ParseBinary(Factor,
			x => new Binary(x, Previous(), Factor()),
			TokenType.MINUS, TokenType.PLUS);
	}

	private Expr Factor()
	{
		return ParseBinary(Unary,
			x => new Binary(x, Previous(), Unary()),
			TokenType.STAR, TokenType.SLASH);
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
		if (Match(TokenType.IDENTIFIER)) return new Variable(Previous());

		if (Match(TokenType.NUMBER, TokenType.STRING)) {
			return new Literal(Previous().Literal);
		}

		if (Match(TokenType.LEFT_PAREN)) {
			var expr = Expression();
			Consume(TokenType.RIGHT_PAREN, "Expect ')' after an expression.");
			return new Grouping(expr);
		}

		throw Error(Peek(), "Expect Expression.");
	}

	private Expr ParseBinary(Func<Expr> rule, Func<Expr, Expr> createNested, params TokenType[] types)
	{
		// ambiguous tokens: MINUS
		if (!Check(TokenType.MINUS) && Check(types))
			throw Error(Peek(), "Left operand is missing.");

		var expr = rule();

		while (Match(types))
		{
			expr = createNested(expr);
		}

		return expr;
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

	private bool Check(params TokenType[] types)
	{
		return types.Any(x => x == Peek().Type);
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