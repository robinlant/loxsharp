namespace loxsharp.Interpreting;

public class LoxClass : ILoxCallable
{
	public string Name { get; set; }

	public int Arity => 0;

	private readonly Dictionary<string, LoxFunction> _methods;

	public LoxClass(string name, Dictionary<string, LoxFunction> methods)
	{
		Name = name;
		_methods = methods;
	}

	public override string ToString()
	{
		return Name;
	}

	public object? Call(Interpreter interpreter, List<object?> arguments)
	{
		return new LoxInstance(this);
	}

	public LoxFunction? GetMethod(string name)
	{
		return _methods.TryGetValue(name, out var method) ? method : null;
	}
}