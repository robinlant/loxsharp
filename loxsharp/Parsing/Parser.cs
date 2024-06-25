using loxsharp.Parsing.Productions;
using loxsharp.Scanning;

namespace loxsharp.Parsing;

public class Parser
{
	private readonly List<Token> _tokens;
	private int _current;
	private bool _isLoop;

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

	public Stmt ParseRepl()
	{
		if (_tokens.Count >= 2 && _tokens[^2].Type is TokenType.SEMICOLON or TokenType.RIGHT_BRACE)
			return Declaration();
		return new Print(Expression());
	}

	private Stmt Declaration()
	{
		try
		{
			return Match(TokenType.VAR) ? VarDeclaration() : Statement();
		}
		catch (ParseException)
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

		if (Match(TokenType.LEFT_BRACE)) return Block();

		if (Match(TokenType.IF)) return If();

		if (Match(TokenType.WHILE)) return While();

		if (Match(TokenType.FOR)) return For();

		if (Match(TokenType.BREAK)) return Break();

		return ExpressionStatement();
	}

	private Stmt Break()
	{
		if (_isLoop is false) Error(Previous(), "Use of 'break' outside of loop is prohibited.");
		Consume(TokenType.SEMICOLON, "Expect ';' after break.");
		return new Break();
	}

//TODO Change break behaviour after add of functions ( throw Error when a break is inside a function body that is inside a function )
	private Stmt For()
	{
		Consume(TokenType.LEFT_PAREN, "Expect '(' after for.");

		Stmt? init = null;
		if (!Match(TokenType.SEMICOLON))
		{
			init = Match(TokenType.VAR)
				? VarDeclaration()
				: ExpressionStatement();
		}

		Expr? condition = null;
		if (!Match(TokenType.SEMICOLON))
		{
			condition = Expression();
			Consume(TokenType.SEMICOLON, "Expect ';' after an if condition.");
		}

		var previous = _isLoop;
		_isLoop = true;

		Expr? incr = null;
		if (!Match(TokenType.RIGHT_PAREN))
		{
			incr = Expression();
			Consume(TokenType.RIGHT_PAREN, "Expect ') just because");
		}

		var stmt = Statement();

		_isLoop = previous;
		return new For(init, condition, incr, stmt);
	}

//TODO Change break behaviour after add of functions ( throw Error when a break is inside a function body that is inside a function )
	private Stmt While()
	{
		Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");

		var condition = Expression();

		Consume(TokenType.RIGHT_PAREN, "Expect ')' after while condition.");

		var previous = _isLoop;
		_isLoop = true;

		var stmt = Statement();

		_isLoop = previous;
		return new While(condition, stmt);
	}

	private Stmt If()
	{
		Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");

		var condition = Expression();

		Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

		var stmt = Statement();

		if (!Match(TokenType.ELSE)) return new If(condition, stmt);

		var elseStmt = Statement();

		return new If(condition, stmt, elseStmt);
	}

	private Stmt Block()
	{
		var statements = new List<Stmt>();

		while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
		{
			statements.Add(Declaration());
		}

		Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
		return new Block(statements);
	}

	private Stmt ExpressionStatement()
	{
		var expression = Expression();

		Consume(TokenType.SEMICOLON, "Expect ';' after value.");

		return new Expression(expression);
	}

	private Stmt PrintStatement()
	{
		var expression = Expression();

		Consume(TokenType.SEMICOLON, "Expect ';' after value.");

		return new Print(expression);
	}

	private Expr Expression()
	{
		return Assignment();
	}

	private Expr Assignment()
	{
		var token = Peek();
		var expr = Comma();

		if (!Match(TokenType.EQUAL)) return expr;
		var equals = Previous();
		var value = Assignment();

		if (expr is Variable) return new Assign(token, value);

		Error(equals, "Invalid assignment target.");

		return null!;
	}

	private Expr Comma()
	{
		return ParseBinary(Conditional,
			x => new Binary(x, Previous(), Conditional()),
			TokenType.COMMA);
	}

	private Expr Conditional()
	{
		var expr = Logic_Or();

		if (!Match(TokenType.QUESTION)) return expr;

		var exprIfTrue = Conditional();

		Consume(TokenType.COLON, "Expect :");

		var exprIfFalse = Conditional();

		return new Conditional(expr, exprIfTrue, exprIfFalse);
	}

	private Expr Logic_Or()
	{
		if (Check(TokenType.OR))
			throw Error(Peek(), "Left operand is missing.");

		var expr = Logic_And();

		while (Match(TokenType.OR))
		{
			expr = new Logical(expr, Previous(), Logic_And());
		}

		return expr;
	}

	private Expr Logic_And()
	{
		if (Check(TokenType.AND))
			throw Error(Peek(), "Left operand is missing.");

		var expr = Equality();

		while (Match(TokenType.AND))
		{
			expr = new Logical(expr, Previous(), Equality());
		}

		return expr;
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
		_isLoop = false;
		Advance();

		while (!IsAtEnd())
		{
			if (Previous().Type is TokenType.SEMICOLON or TokenType.RIGHT_BRACE) return;

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