using loxsharp.Scanning;

namespace loxsharp.Interpreting;

public class Environment
{
	private readonly Dictionary<string, object?> _dictionary = new Dictionary<string, object?>();

	public void Define(Token token, object? value = null)
	{
		if (_dictionary.TryAdd(token.Lexeme, value)) return;

		throw new RuntimeException(token, $"Variable {token.Lexeme} already exist.");
	}

	public void Assign(Token token, object? value)
	{
		if (!_dictionary.ContainsKey(token.Lexeme))
			throw new RuntimeException(token, $"Variable {token.Lexeme} isn't declared");

		_dictionary[token.Lexeme] = value;
	}

	public object? Get(Token token)
	{
		var isSuccess = _dictionary.TryGetValue(token.Lexeme, out var result);

		if (isSuccess) return result;

		throw new RuntimeException(token, "Undefined variable '" + token.Lexeme + "'.");
	}
}