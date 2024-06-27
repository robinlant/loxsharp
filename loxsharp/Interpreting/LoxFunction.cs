using loxsharp.Parsing.Productions;
using loxsharp.Scanning;

namespace loxsharp.Interpreting;

public class LoxFunction : ILoxCallable
{
	public int Arity => _params.Count;

	private readonly string? _name;
	private readonly List<Token> _params;
	private readonly List<Stmt> _statements;

	private readonly Environment _clojure;

	public LoxFunction(Function declaration, Environment clojure)
	{
		_name = declaration.Name.Lexeme;
		_params = declaration.Params;
		_statements = declaration.Body;
		_clojure = clojure;
	}

	public LoxFunction(Lambda lambda, Environment clojure)
	{
		_params = lambda.Params;
		_statements = lambda.Statements;
		_clojure = clojure;
	}

	public object? Call(Interpreter interpreter, List<object?> arguments)
	{
		var environment = new Environment(_clojure);

		for(var i = 0; i < arguments.Count; i++)
		{
			environment.Define(_params[i], arguments[i]);
		}

		interpreter.ExecuteBlock(_statements, environment);
		return null;
	}

	public override string ToString()
	{
		return _name is null ? "<anonymous fn>" : "<fn " + _name + ">";
	}
}