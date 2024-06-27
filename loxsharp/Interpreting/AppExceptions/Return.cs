using loxsharp.Scanning;

namespace loxsharp.Interpreting.AppExceptions;

public class ReturnException : RuntimeException
{
	public object? Value { get; }

	public ReturnException(Token token, object? value)
		: base(token, "Use of 'return' outside of function or method body.")
	{
		Value = value;
	}
}