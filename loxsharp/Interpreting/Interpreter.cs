using loxsharp.Parsing;
using loxsharp.Parsing.Productions;
using loxsharp.Scanning;
using Expression = loxsharp.Parsing.Productions.Expression;

namespace loxsharp.Interpreting;

public class Interpreter : ISyntaxTreeVisitor<object?>, IStatementVisitor<Interpreter.Nothing?>
{
	public class Nothing{}

	private readonly Action<RuntimeException> _error;

	private Environment _environment = new Environment();

	public Interpreter(Action<RuntimeException> error)
	{
		_error = error;
	}

	public void Interpret(List<Stmt> statements)
	{
		try
		{
			foreach (var i in statements)
			{
				// execute
				i.Accept(this);
			}

		}
		catch (RuntimeException e)
		{
			_error(e);
		}
	}

	public void InterpretRepl(Stmt statement)
	{
		try
		{
			statement.Accept(this);

		}
		catch (RuntimeException e)
		{
			_error(e);
		}
	}

	public object? VisitAssign(Assign assign)
	{
		var value = assign.Value.Accept(this);

		_environment.Assign(assign.Token,value);

		return value;
	}

	public object? VisitBinary(Binary binary)
	{
		var left = binary.Left.Accept(this);
		var right = binary.Right.Accept(this);

		switch (binary.Token.Type)
		{
			case TokenType.PLUS:
				if (CheckTypes(typeof(double), left, right))
				{
					return (double)left! + (double)right!;
				}

				if (CheckTypes(typeof(string), left) || CheckTypes(typeof(string), right))
				{
					return Stringify(left) + Stringify(right);
				}

				throw new RuntimeException(binary.Token, "Operands must be two numbers or two strings.");
			case TokenType.MINUS:
				CheckNumberOperands(binary.Token, left, right);
				return (double)left! - (double)right!;
			case TokenType.SLASH:
				CheckNumberOperands(binary.Token, left, right);
				if (right is double and 0) throw new RuntimeException(binary.Token, "Division by zero is prohibited.");
				return (double)left! / (double)right!;
			case TokenType.STAR:
				CheckNumberOperands(binary.Token, left, right);
				return (double)left! * (double)right!;
			case TokenType.GREATER:
				CheckNumberOperands(binary.Token, left, right);
				return (double)left! > (double)right!;
			case TokenType.GREATER_EQUAL:
				CheckNumberOperands(binary.Token, left, right);
				return (double)left! >= (double)right!;
			case TokenType.LESS:
				CheckNumberOperands(binary.Token, left, right);
				return (double)left! < (double)right!;
			case TokenType.LESS_EQUAL:
				CheckNumberOperands(binary.Token, left, right);
				return (double)left! <= (double)right!;
			case TokenType.BANG_EQUAL:
				return !IsEqual(left, right);
			case TokenType.EQUAL_EQUAL:
				return IsEqual(left, right);
			case TokenType.COMMA:
				return right;
		}

		return null;
	}

	public object? VisitGrouping(Grouping grouping)
	{
		return grouping.Expression.Accept(this);
	}

	public object? VisitLiteral(Literal literal)
	{
		return literal.Value;
	}

	public object? VisitLogical(Logical logical)
	{
		if (logical.Token.Type == TokenType.OR)
		{
			return IsTruthy(logical.Left.Accept(this)) ||
			       IsTruthy(logical.Right.Accept(this));
		}
		else if (logical.Token.Type == TokenType.AND)
		{
			return IsTruthy(logical.Left.Accept(this)) &&
			       IsTruthy(logical.Right.Accept(this));
		}

		return null;
	}

	public object? VisitUnary(Unary unary)
	{
		var right = unary.Right.Accept(this);

		switch (unary.Token.Type)
		{
			case TokenType.MINUS:
				CheckNumberOperands(unary.Token, right);
				return -(double)right!;
			case TokenType.BANG:
				return !IsTruthy(right);
			default:
				return null;
		}
	}

	public object? VisitConditional(Conditional conditional)
	{
		var condition = conditional.Condition.Accept(this);

		return IsTruthy(condition)
			? conditional.ExprIfTrue.Accept(this)
			: conditional.ExprIfFalse.Accept(this);
	}

	public object? VisitVariable(Variable variable)
	{
		return _environment.Get(variable.Token);
	}

	private bool IsTruthy(object? parameter)
	{
		// null or false => false, otherwise true
		return parameter is not null && (parameter is not bool b || b);
	}

	private void CheckNumberOperands(Token token, params object?[] operands)
	{
		if (CheckTypes(typeof(double), operands)) return;
		throw new RuntimeException(token, "Operand must be a number.");
	}

	private bool IsEqual(object? a, object? b)
	{
		return a switch
		{
			null when b is null => true,
			null => false,
			_ => a.Equals(b)
		};
	}

	private bool CheckTypes(Type type, params object?[] objects)
	{
		return objects.All(x => x?.GetType() == type);
	}

	private string Stringify(object? obj)
	{
		if (obj is null) return "nil";

		if (CheckTypes(typeof(double), obj))
		{
			var text = obj.ToString();

			if (text!.EndsWith(".0"))
			{
				text = text[..^2];
			}

			return text;
		}

		return obj.ToString() ?? "";
	}

	public Nothing VisitExpression(Expression expression)
	{
		expression.Expr.Accept(this);

		return new Nothing();
	}

	public Nothing VisitPrint(Print print)
	{
		var value = print.Expr.Accept(this);

		Console.WriteLine(Stringify(value));

		return new Nothing();
	}

	public Nothing VisitVar(Var var)
	{
		if (var.Init is null)
			_environment.Define(var.Token);
		else
			_environment.Define(var.Token, var.Init.Accept(this));

		return new Nothing();
	}

	public Nothing VisitBlock(Block block)
	{
		ExecuteBlock(block.Stmts, new Environment(_environment));

		return new Nothing();
	}

	public Nothing? VisitIf(If ifStmt)
	{
		var condition = ifStmt.Condition.Accept(this);

		if (IsTruthy(condition))
		{
			ifStmt.ThenStmt.Accept(this);

			return new Nothing();
		}

		if (ifStmt.ElseStmt is null) return new Nothing();

		ifStmt.ElseStmt.Accept(this);

		return new Nothing();
	}

	public Nothing? VisitWhile(While whileStmt)
	{
		while (IsTruthy(whileStmt.Condition.Accept(this)))
		{
			whileStmt.ThenStmt.Accept(this);
		}

		return new Nothing();
	}

	public Nothing? VisitFor(For forStmt)
	{
		var previous = _environment;
		try
		{
			_environment = new Environment(previous);
			forStmt.Init?.Accept(this);
			while (forStmt.Condition is null
			       || IsTruthy(forStmt.Condition.Accept(this)))
			{
				forStmt.Stmt.Accept(this);
				forStmt.Increment?.Accept(this);
			}

		}
		finally
		{
			_environment = previous;
		}

		return new Nothing();
	}

	private void ExecuteBlock(List<Stmt> statements,Environment environment)
	{
		var previous = _environment;
		try
		{
			_environment = environment;
			foreach (var stmt in statements)
			{
				stmt.Accept(this);
			}
		}
		finally
		{
			_environment = previous;
		}
	}
}