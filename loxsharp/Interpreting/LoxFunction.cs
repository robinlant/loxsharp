using loxsharp.Interpreting.AppExceptions;
using loxsharp.Parsing.Productions;
using loxsharp.Scanning;

namespace loxsharp.Interpreting;

public class LoxFunction : ILoxCallable
{
	public int Arity => _params.Count;

	private readonly string? _name;
	private readonly List<Token> _params;
	private readonly List<Stmt> _statements;
	public bool IsInit { get; set; }

	private readonly Environment _clojure;

	public LoxFunction(Function declaration, Environment clojure, bool isInit = false)
	{
		_name = declaration.Name.Lexeme;
		_params = declaration.Params;
		_statements = declaration.Body;
		_clojure = clojure;
		IsInit = isInit;
	}

	public LoxFunction(Lambda lambda, Environment clojure, bool isInit = false)
	{
		_params = lambda.Params;
		_statements = lambda.Statements;
		_clojure = clojure;
		IsInit = isInit;
	}

	public LoxFunction(string? name, List<Token> @params, List<Stmt> statements, Environment clojure, bool isInit = false)
	{
		_name = name;
		_params = @params;
		_statements = statements;
		IsInit = isInit;
		_clojure = clojure;
	}

	public object? Call(Interpreter interpreter, List<object?> arguments)
	{
		var environment = new Environment(_clojure);

		for(var i = 0; i < arguments.Count; i++)
		{
			environment.Define(_params[i], arguments[i]);
		}

		try
		{
			interpreter.ExecuteBlock(_statements, environment);
		}
		catch (ReturnException e)
		{
			if (IsInit)
				return _clojure.GetAt(0, new Token(TokenType.THIS, "this", null, -2));
			throw;
		}

		return IsInit
			? _clojure.GetAt(0,new Token(TokenType.THIS, "this", null, -2))
			: null;
	}

	public override string ToString()
	{
		return _name is null ? "<anonymous fn>" : "<fn " + _name + ">";
	}

	public LoxFunction Bind(LoxInstance instance)
	{
		var environment = new Environment(_clojure);
		environment.Define(new Token(TokenType.THIS, "this", null, -1), instance);
		return new LoxFunction(_name, _params, _statements, environment, IsInit);
	}
}