namespace loxsharp.Interpreting;

public interface ILoxCallable
{
	int Arity { get; }

	object? Call(Interpreter interpreter, List<object?> arguments);
}