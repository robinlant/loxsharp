using System.Text;
using Environment = System.Environment;

namespace loxsharp;

public class Program
{
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

	private static void RunFile(string file)
	{
		if (!File.Exists(file))
		{
			Console.WriteLine(
				$"Cant open the file: {file}." +
				$"\nFile doesnt exist.");
			Environment.Exit(66);
		}

		var bytes = File.ReadAllBytes(file);

		Run(Encoding.UTF8.GetString(bytes));
	}

	private static void RunPrompt()
	{
		for (;;)
		{
			Console.Write("> ");
			var line = Console.ReadLine();
			if (line is null) break;
			Run(line);
		}
	}

	private static void Run(string source)
	{
		// var scanner = new Scanner();
		// var tokens = Scanner.scanTockens(source);
	}
}