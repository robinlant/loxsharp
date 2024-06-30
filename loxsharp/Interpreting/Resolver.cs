using loxsharp.Parsing;
using loxsharp.Parsing.Productions;
using loxsharp.Scanning;

namespace loxsharp.Interpreting;

public class Resolver : ISyntaxTreeVisitor<Resolver.Unit>, IStatementVisitor<Resolver.Unit>
{
	public struct Unit {}

	private readonly List<Dictionary<string, VariableInfo>> _scopes = new();

	private readonly Action<int, string> _reportError;

	private readonly Interpreter _interpreter;

	private FunctionType _functionType = FunctionType.None;

	public Resolver(Interpreter interpreter ,Action<int, string> error)
	{
		_interpreter = interpreter;
		_reportError = error;
	}

	public Unit VisitAssign(Assign assign)
	{
		Resolve(assign.Value);
		ResolveLocal(assign, assign.Token);
		return new Unit();
	}

	public Unit VisitBinary(Binary binary)
	{
		Resolve(binary.Left);
		Resolve(binary.Right);
		return new Unit();
	}

	public Unit VisitCall(Call call)
	{
		Resolve(call.Callee);
		foreach (var i in call.Arguments)
		{
			Resolve(i);
		}
		return new Unit();
	}

	public Unit VisitGrouping(Grouping grouping)
	{
		Resolve(grouping.Expression);
		return new Unit();
	}

	public Unit VisitLiteral(Literal literal)
	{
		return new Unit();
	}

	public Unit VisitLogical(Logical logical)
	{
		Resolve(logical.Left);
		Resolve(logical.Right);
		return new Unit();
	}

	public Unit VisitUnary(Unary unary)
	{
		Resolve(unary.Right);
		return new Unit();
	}

	public Unit VisitConditional(Conditional conditional)
	{
		Resolve(conditional.Condition);
		Resolve(conditional.ExprIfTrue);
		Resolve(conditional.ExprIfFalse);
		return new Unit();
	}

	//TODO error if var is not in the map
	public Unit VisitVariable(Variable variable)
	{
		if (_scopes.Count != 0
		    && _scopes[^1].TryGetValue(variable.Token.Lexeme, out var res)
		    &&!res.IsDefined)
		{
			Error(variable.Token, "Can't read local variable in its own initializer.");
		}

		ResolveLocal(variable, variable.Token);
		return new Unit();
	}

	public Unit VisitLambda(Lambda lambda)
	{
		ResolveLoxFunction(lambda.Params, lambda.Statements, FunctionType.Lambda);
		return new Unit();
	}

	public Unit VisitExpression(Expression expression)
	{
		Resolve(expression.Expr);
		return new Unit();
	}

	public Unit VisitPrint(Print print)
	{
		Resolve(print.Expr);
		return new Unit();
	}

	public Unit VisitVar(Var var)
	{
		Declare(var.Token);
		if (var.Init is not null)
		{
			Resolve(var.Init);
		}

		Define(var.Token);
		return new Unit();
	}

	public Unit VisitBlock(Block block)
	{
		BeginScope();
		Resolve(block.Stmts);
		EndScope();
		return new Unit();
	}

	public Unit VisitIf(If ifStmt)
	{
		Resolve(ifStmt.Condition);
		Resolve(ifStmt.ThenStmt);
		if (ifStmt.ElseStmt is not null) Resolve(ifStmt.ElseStmt);
		return new Unit();
	}

	public Unit VisitWhile(While whileStmt)
	{
		Resolve(whileStmt.Condition);
		Resolve(whileStmt.ThenStmt);
		return new Unit();
	}

	public Unit VisitFor(For forStmt)
	{
		if (forStmt.Init is not null) Resolve(forStmt.Init);
		if (forStmt.Condition is not null) Resolve(forStmt.Condition);
		if (forStmt.Increment is not null) Resolve(forStmt.Increment);
		Resolve(forStmt.Stmt);
		return new Unit();
	}

	public Unit VisitBreak(Break breakStmt)
	{
		return new Unit();
	}

	public Unit VisitFunction(Function function)
	{
		Declare(function.Name);
		Define(function.Name);

		ResolveLoxFunction(function.Params, function.Body, FunctionType.Function);
		return new Unit();
	}

	public Unit VisitReturn(Return returnStmt)
	{
		if (_functionType == FunctionType.None)
			Error(returnStmt.Token, "Use of 'return' outside of function or method body.");

		if(returnStmt.Value is not null) Resolve(returnStmt.Value);
		return new Unit();
	}

	private void BeginScope()
	{
		_scopes.Add(new Dictionary<string, VariableInfo>());
	}

	private void EndScope()
	{
		foreach (var variable in _scopes[^1].Where(variable => !variable.Value.IsUsed))
		{
			Error(new Token(TokenType.IDENTIFIER,variable.Key,null,-1), $"Local variable '{variable.Key}' is never used.");
		}

		_scopes.RemoveAt(_scopes.Count - 1);
	}

	public void Resolve(IEnumerable<Stmt> stmts)
	{
		foreach (var i in stmts)
		{
			Resolve(i);
		}
	}

	public void Resolve(Stmt stmt)
	{
		stmt.Accept(this);
	}

	public void Resolve(Expr expr)
	{
		expr.Accept(this);
	}

	private void Declare(Token token)
	{
		if (_scopes.Count == 0) return;

		if (_scopes[^1].ContainsKey(token.Lexeme))
		{
			Error(token, "Already a variable with this name in this scope.");
			return;
		}

		_scopes[^1].Add(token.Lexeme, new VariableInfo());
	}

	private void Define(Token token)
	{
		if (_scopes.Count == 0) return;

		_scopes[^1][token.Lexeme] = _scopes[^1][token.Lexeme] with { IsDefined = true };
	}

	private void Error(Token token, string message)
	{
		_reportError(token.Line, message);
	}

	private void ResolveLocal(Expr expr, Token token)
	{
		for (var i = _scopes.Count - 1; i >= 0; i--)
		{
			if (!_scopes[i].ContainsKey(token.Lexeme)) continue;
			_scopes[i][token.Lexeme] = _scopes[i][token.Lexeme] with { IsUsed = true };
			_interpreter.Resolve(expr, _scopes.Count - 1 - i);
			return;
		}
	}

	private void ResolveLoxFunction(List<Token> parameters, IEnumerable<Stmt> stmts, FunctionType functionType)
	{
		var enclosingFuncType = _functionType;
		_functionType = functionType;

		BeginScope();
		foreach (var i in parameters)
		{
			Declare(i);
			Define(i);
		}

		Resolve(stmts);

		EndScope();

		_functionType = enclosingFuncType;
	}

	private enum FunctionType
	{
		None,
		Function,
		Lambda,

	}

	private struct VariableInfo
	{
		public bool IsDefined { get; set; }

		public bool IsUsed { get; set; }
	}
}