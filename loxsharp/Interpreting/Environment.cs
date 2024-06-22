using loxsharp.Scanning;

namespace loxsharp.Interpreting;

public class Environment
{
	private readonly Dictionary<string, object?> _dictionary = new Dictionary<string, object?>();

	public void Define(string name, object? value = null)
	{
		if (_dictionary.TryAdd(name, value))
			_dictionary[name] = value;
	}

	public object? Get(Token name)
	{
		var isSuccess = _dictionary.TryGetValue(name.Lexeme, out var result);

		if (isSuccess) return result;

		throw new RuntimeException(name, "Undefined variable '" + name.Lexeme + "'.");
	}
}