using System.Diagnostics;

namespace loxsharp.Interpreting;

public class LoxClass : ILoxCallable
{
	public string Name { get; set; }

	public int Arity => GetMethod("init") is { } function ? function.Arity : 0;

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
		var instance = new LoxInstance(this);
		var init = GetMethod("init");
		// Init is just a function with name init()
		// to use it we firstly add this keyword via Bind()
		// then we call method and return an object
		if (init is null) return instance;
		init.IsInit = true;
		init.Bind(instance).Call(interpreter, arguments);
		return instance;
	}

	public LoxFunction? GetMethod(string name)
	{
		return _methods.TryGetValue(name, out var method) ? method : null;
	}
}