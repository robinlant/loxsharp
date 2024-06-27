using loxsharp.Parsing.Productions;

namespace loxsharp.Interpreting;

public class LoxFunction : ILoxCallable
{
	public int Arity => _declaration.Params.Count;

	private readonly Function _declaration;

	public LoxFunction(Function declaration)
	{
		_declaration = declaration;
	}

	public object? Call(Interpreter interpreter, List<object?> arguments)
	{
		var environment = new Environment(interpreter.Globals);

		for(var i = 0; i < arguments.Count; i++)
		{
			environment.Define(_declaration.Params[i], arguments[i]);
		}

		interpreter.ExecuteBlock(_declaration.Body, environment);
		return null;
	}

	public override string ToString()
	{
		return "<fn " + _declaration.Name.Lexeme + ">";
	}
}