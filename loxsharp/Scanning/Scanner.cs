namespace loxsharp.Scanning;

public class Scanner
{
	private static readonly Dictionary<string, TokenType> Keywords
		= new Dictionary<string, TokenType>();
	private readonly string _source;
	private readonly List<Token> _tokens = new List<Token>();
	private int _start;
	private int _current;
	private int _line = 1;

	private readonly Action<int, string> _reportError;

	static Scanner()
	{
		// Reserved Keywords
		Keywords.Add("and", TokenType.AND);
		Keywords.Add("class", TokenType.CLASS);
		Keywords.Add("else", TokenType.ELSE);
		Keywords.Add("false", TokenType.FALSE);
		Keywords.Add("fun", TokenType.FUN);
		Keywords.Add("for", TokenType.FOR);
		Keywords.Add("if", TokenType.IF);
		Keywords.Add("nil", TokenType.NIL);
		Keywords.Add("or", TokenType.OR);
		Keywords.Add("print", TokenType.PRINT);
		Keywords.Add("return", TokenType.RETURN);
		Keywords.Add("super", TokenType.SUPER);
		Keywords.Add("this", TokenType.THIS);
		Keywords.Add("true", TokenType.TRUE);
		Keywords.Add("var", TokenType.VAR);
		Keywords.Add("while", TokenType.WHILE);
	}

	public Scanner(string source, Action<int,string> reportError)
	{
		_source = source;
		_reportError = reportError;
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
			case '?': AddToken(TokenType.QUESTION); break;
			case ':': AddToken(TokenType.COLON); break;
			case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
			case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
			case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
			case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
			case '/':
				if (Peek() == ('/'))
				{
					while (Peek() != '\n' && !IsAtEnd()) Advance();
				}
				else if (Match('*')) // Block comments
				{
					BlockComment();
				}
				else
				{
					AddToken(TokenType.SLASH);
				}
				break;
			case ' ':
			case '\r':
			case '\t':
				break; // Ignore Whitespaces
			case '\n':
				_line++;
				break;
			case '"': String(); break;

			default:
				if (IsDigit(c))
				{
					Number();
				}
				else if (IsAlpha(c))
				{
					Identifier();
				}
				else
				{
					_reportError(_line, "Unexpected character.");
				}

				break;
		}
	}

	private bool IsDigit(char c)
	{
		return c is >= '0' and <= '9';
	}

	private bool IsAlpha(char c)
	{
		return c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '_';
	}

	private void Identifier()
	{
		while (IsAlpha(Peek())) Advance();

		var identifier = _source.Substring(_start, _current - _start);

		if (Scanner.Keywords.TryGetValue(identifier, out var tokenType))
		{
			AddToken(tokenType);
			return;
		}

		AddToken(TokenType.IDENTIFIER, _source.Substring(_start, _current - _start));
	}

	private void String()
	{
		while (Peek() != '"' && !IsAtEnd())
		{
			if (Peek() == '\n') _line++;
			Advance();
		}

		if (IsAtEnd())
		{
			_reportError(_line, "Unterminated string");
			return;
		}

		Advance();
		var value = _source.Substring(_start + 1, _current - _start - 2);
		AddToken(TokenType.STRING, value);
	}

	private void Number()
	{
		while (IsDigit(Peek())) Advance();

		if (Peek() == '.' && IsDigit(PeekNext())) Advance(); // consume '.'

		while (IsDigit(Peek())) Advance();

		AddToken(TokenType.NUMBER, Double.Parse(_source.Substring(_start, _current - _start)));
	}

	private void BlockComment()
	{
		var nest = 1;

		while (nest != 0 && !IsAtEnd())
		{
			var c = Advance();
			switch (c)
			{
				case '\n': _line++; break;
				case '*': if (Match('/')) nest--; break;
				case '/': if (Match('*')) nest++; break;
			}
		}

		if (IsAtEnd())
			_reportError(_line, "Unterminated block comment");
	}

	private char PeekNext()
	{
		return _current + 1 >= _source.Length
			? '\0'
			: _source[_current + 1];
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
		var lexeme = _source.Substring(_start, _current - _start);
		_tokens.Add(new Token(type, lexeme, literal, _line));
	}

	private bool IsAtEnd()
	{
		return _current >= _source.Length;
	}
}