using System.Runtime.InteropServices.ComTypes;

namespace loxsharp.Interpreting.Globals;

public class Clock : ILoxCallable
{
	public int Arity { get; } = 0;

	//returns count of milliseconds since 1st January 1970
	public object? Call(Interpreter interpreter, List<object?> arguments)
	{
		var now = DateTime.UtcNow;
		var jsTime = new DateTime(1970, 1, 1).ToUniversalTime();

		return (now - jsTime).TotalMilliseconds;
	}

	public override string ToString()
	{
		return $"<native fun {this.GetType().Name}>";
	}
}