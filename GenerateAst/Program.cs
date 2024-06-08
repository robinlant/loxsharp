if (args.Length != 1)
{
	Console.WriteLine("Usage: generate_ast <output directory>");
	Environment.Exit(64);
}

var outputDir = args[0];

new GenerateAst.GenerateAst(outputDir, "Expr", new[]
{
	"Binary   : Expr Left, Token Token, Expr Right",
	"Grouping : Expr Expression",
	"Literal  : Object Value",
	"Unary    : Token Token, Expr Right"
}).Generate();