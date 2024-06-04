using System.Runtime.InteropServices.JavaScript;

namespace loxsharp;

public class Scanner
{
	private readonly string _source;
	private readonly List<Token> _tokens = new List<Token>();
	private int _start = 0;
	private int _current = 0;
	private int _line = 0;

	public Scanner(string source)
	{
		_source = source;
	}

	public List<Token> ScanTokens()
	{
		while (!IsAtEnd())
		{
			_start = _current;
			ScanToken();
		}

		_tokens.Add(new Token(TokenType.EOF, "", null, _line));
		return _tokens;
	}

	private void ScanToken()
	{
		var c = Advance();
		switch (c) {
			case '(': AddToken(TokenType.LEFT_PAREN); break;
			case ')': AddToken(TokenType.RIGHT_PAREN); break;
			case '{': AddToken(TokenType.LEFT_BRACE); break;
			case '}': AddToken(TokenType.RIGHT_BRACE); break;
			case ',': AddToken(TokenType.COMMA); break;
			case '.': AddToken(TokenType.DOT); break;
			case '-': AddToken(TokenType.MINUS); break;
			case '+': AddToken(TokenType.PLUS); break;
			case ';': AddToken(TokenType.SEMICOLON); break;
			case '*': AddToken(TokenType.STAR); break;
			case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
			case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
			case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
			case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
			case '/':
				if (Peek() == '/')
				{
					while (Peek() != '\0' && !IsAtEnd()) Advance();
				} else {
					AddToken(TokenType.SLASH);
				}
				break;
			case ' ':
			case '\r':
			case '\t':
				// Ignore whitespace.
				break;

			case '\n':
				_line++;
				break;


			default:
				Program.Error(_line, "Unexpected character.");
				break;
		}
	}

	private char Peek()
	{
		return IsAtEnd() ? '\0' : _source[_current];
	}

	private bool Match(char expected)
	{
		if (IsAtEnd()) return false;
		if (_source[_current] != expected) return false;

		_current++;
		return true;
	}

	private char Advance()
	{
		return _source[_current++];
	}

	private void AddToken(TokenType type, object? literal = null)
	{
		var lexeme = _source.Substring(_start, _current);
		_tokens.Add(new Token(type, lexeme, literal, _line));
	}

	private bool IsAtEnd()
	{
		return _current >= _source.Length;
	}
}