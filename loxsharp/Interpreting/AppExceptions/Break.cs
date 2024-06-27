using loxsharp.Scanning;

namespace loxsharp.Interpreting.AppExceptions;

public class BreakException : RuntimeException
{
	public BreakException(Token token)
		: base(token, "Use of 'break' outside of loop body.") {}
}