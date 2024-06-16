namespace loxsharp.Parsing;

public class ParseException : ApplicationException
{
	public ParseException(){}

	public ParseException(string message) : base(message){}
}