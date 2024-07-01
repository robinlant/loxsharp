if (args.Length != 1)
{
	Console.WriteLine("Usage: generate_ast <output directory>");
	Environment.Exit(64);
}

var outputDir = args[0];

new GenerateAst.GenerateAst(outputDir, "Expr", new[]
{
	"Assign   : Token Token, Expr value",
	"Binary   : Expr Left, Token Token, Expr Right",
	"Call	  : Expr Callee, Token Paren, List<Expr> arguments",
	"Get      : Expr Object, Token Token",
	"Grouping : Expr Expression",
	"Literal  : object? Value",
	"Logical  : Expr left, Token operator, Expr right",
	"Set	  : Expr Object, Token Token, Expr Value",
	"This     : Token Token",
	"Unary    : Token Token, Expr Right",
	"Conditional : Expr Condition, Expr ExprIfTrue, Expr ExprIfFalse",
	"Variable : Token Token",
	"Lambda   : Token Token, List<Token> Params, List<Stmt> Statements",
}).Generate();