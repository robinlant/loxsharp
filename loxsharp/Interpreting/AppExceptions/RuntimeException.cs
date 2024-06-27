using loxsharp.Scanning;

namespace loxsharp.Interpreting;

public class RuntimeException : Exception
{
	public Token Token { get; init; }

	public RuntimeException(Token token, string message)
		: base(message)
	{
		Token = token;
	}
}