using loxsharp.Scanning;

namespace loxsharp.Interpreting;

public class Environment
{
	private readonly Dictionary<string, object?> _dictionary = new Dictionary<string, object?>();
	private readonly Environment? _enclosing;

	public Environment()
	{
		_enclosing = null;
	}

	public Environment(Environment enclosing)
	{
		_enclosing = enclosing;
	}

	public void Define(Token token, object? value = null)
	{
		if (_dictionary.TryAdd(token.Lexeme, value)) return;

		throw new RuntimeException(token, $"Variable {token.Lexeme} is already defined.");
	}

	public void Assign(Token token, object? value)
	{
		if (_dictionary.ContainsKey(token.Lexeme))
		{
			_dictionary[token.Lexeme] = value;
			return;
		}
		else if(_enclosing is not null)
		{
			_enclosing.Assign(token, value);
			return;
		}

		throw new RuntimeException(token, "Undefined variable '" + token.Lexeme + "'.");
	}

	public object? Get(Token token)
	{
		var isSuccess = _dictionary.TryGetValue(token.Lexeme, out var result);

		if (isSuccess) return result;
		else if (_enclosing is not null) return _enclosing.Get(token);

		throw new RuntimeException(token, "Undefined variable '" + token.Lexeme + "'.");
	}
}