using System.Text;
using loxsharp.Interpreting;
using loxsharp.Parsing;
using loxsharp.Scanning;
using Environment = System.Environment;

namespace loxsharp;

public static class Program
{
	private static bool _hadError = false;

	public static void Main(string[] args)
	{
		Console.WriteLine();
		switch (args.Length)
		{
			case > 1:
				Console.WriteLine("Usage: loxsharp [script]");
				Environment.Exit(64);
				break;
			case 1:
				RunFile(args[0]);
				break;
			default:
				RunPrompt();
				break;
		}
	}

	private static void RuntimeError(RuntimeException exception)
	{
		Error(exception.Token.Line, exception.Message);
	}

	private static void Error(int line, string message)
	{
		Report(line, "", message);
		_hadError = true;
	}

	private static void RunFile(string file)
	{
		if (!File.Exists(file))
		{
			Console.WriteLine(
				$"Cant open the file: {file}" +
				$"\nFile doesnt exist.");
			Environment.Exit(66);
		}

		var bytes = File.ReadAllBytes(file);

		Run(Encoding.UTF8.GetString(bytes));

		if(_hadError) Environment.Exit(65);
	}

	private static void RunPrompt()
	{
		for (;;)
		{
			Console.Write("> ");
			var line = Console.ReadLine();
			if (line is null) break;
			Run(line);
			_hadError = false;
		}
	}

	private static void Run(string source)
	{
		var scanner = new Scanner(source, Error);
		var tokens = scanner.ScanTokens();
		var parser = new Parser(tokens, Error);
		var stmts = parser.Parse();
		var interpreter = new Interpreter(RuntimeError);
		interpreter.Interpret(stmts);
	}

	private static void Report(int line, string where, string message)
	{
		Console.Error.Write($"[line: {line}] Error{where}: {message}\n");
	}
}