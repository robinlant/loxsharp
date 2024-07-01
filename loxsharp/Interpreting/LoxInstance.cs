using loxsharp.Scanning;

namespace loxsharp.Interpreting;

public class LoxInstance
{
	private readonly LoxClass _loxClass;
	private readonly Dictionary<string, object?> _fields = new ();

	public LoxInstance(LoxClass loxClass)
	{
		_loxClass = loxClass;
	}

	public override string ToString()
	{
		return _loxClass.Name + " instance";
	}

	public object? Get(Token token)
	{
		if (_fields.TryGetValue(token.Lexeme, out var res))
			return res;

		var method = _loxClass.GetMethod(token.Lexeme);
		if (method is not null) return method.Bind(this);

		throw new RuntimeException(token, "Undefined property '" + token.Lexeme + "'.");
	}

	public void Set(Token token, object? value)
	{
		_fields[token.Lexeme] = value;
	}
}