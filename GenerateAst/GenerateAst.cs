using System.Text;

namespace GenerateAst;

public class GenerateAst
{
	private readonly string _outputDir;
	private readonly string _baseName;
	private readonly IEnumerable<TypeInformation> _types;

	public GenerateAst(string outputDir, string baseName, IEnumerable<string> types)
	{
		_outputDir = outputDir;
		_baseName = baseName;
		_types = types.Select(x => new TypeInformation(x));
	}

	public void Generate()
	{
		var path = _outputDir + Path.DirectorySeparatorChar + _baseName + ".cs";
		var stringBuilder = new StringBuilder();

		WriteToAFile(_outputDir,"ISyntaxTreeVisitor", GenerateVisitorInterface(true));

		stringBuilder.AppendLine(GenerateMainClass());

		foreach (var i in _types)
		{
			stringBuilder.AppendLine(GenerateType(i));
		}

		WriteToAFile(_outputDir + Path.DirectorySeparatorChar + "Productions",_baseName, stringBuilder.ToString());
	}

	private string GenerateType(TypeInformation type ,bool withNameSpaceAndUsing = false)
	{
		var textBuilder = new StringBuilder();
		if (withNameSpaceAndUsing) textBuilder.AppendLine("namespace loxsharp.Parser;\n");
		textBuilder.AppendLine($"public record {type.Name}(Expr Left, Token Token, Expr Right) : {_baseName}");
		textBuilder.AppendLine("{");
		textBuilder.AppendLine("   " + "public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)");
		textBuilder.AppendLine("   " + "{");
		textBuilder.AppendLine("   " + "   " + $"return visitor.Visit{type.Name}(this);");
		textBuilder.AppendLine("   " + "}");
		textBuilder.AppendLine("}");

		return textBuilder.ToString();
	}

	private string GenerateMainClass()
	{
		var textBuilder = new StringBuilder();
		textBuilder.AppendLine("using loxsharp.Scanner;\n");
		textBuilder.AppendLine("namespace loxsharp.Parser.Productions;\n");
		textBuilder.AppendLine($"public abstract record {_baseName}");
		textBuilder.AppendLine("{");
		textBuilder.AppendLine("    "+"public abstract T Accept<T>(ISyntaxTreeVisitor<T> visitor);");
		textBuilder.AppendLine("}");

		return textBuilder.ToString();
	}

	private string GenerateVisitorInterface(bool withNameSpaceAndUsing = false)
	{
		var textStrBuilder = new StringBuilder();
		if (withNameSpaceAndUsing)
		{
			textStrBuilder.AppendLine("using loxsharp.Parser.Productions;\n");
			textStrBuilder.AppendLine("namespace loxsharp.Parser;\n");
		}

		textStrBuilder.AppendLine("public interface ISyntaxTreeVisitor<T>");
		textStrBuilder.AppendLine("{");

		foreach (var i in _types)
		{
			var paramStrBuilder = new StringBuilder(i.Name);
			paramStrBuilder[0] = char.ToLower(paramStrBuilder[0]);
			textStrBuilder.AppendLine("    " + $"T Visit{i.Name}({i.Name} {paramStrBuilder.ToString()});\n");
		}

		textStrBuilder[^1] = '}';
		return textStrBuilder.ToString();
	}

	private void WriteToAFile(string directory, string filename, string text)
	{
		if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

		var path = directory + Path.DirectorySeparatorChar + filename + ".cs";

		if (File.Exists(path)) File.Delete(path);

		File.WriteAllText(path,text, Encoding.UTF8);
	}

	private record TypeInformation(string Name, IEnumerable<string> Properties)
	{
		public TypeInformation(string type) : this(ParseName(type), ParseProperties(type)) { }

		private static IEnumerable<string> ParseProperties(string type)
		{
			return type.Split(":")[1].Trim().Split(", ").Select(x =>
			{
				var strBuilder = new StringBuilder(x);
				var indexOf = x.IndexOf(" ", StringComparison.Ordinal) + 1;
				strBuilder[indexOf] = char.ToUpper(strBuilder[indexOf]);
				return strBuilder.ToString();
			});
		}

		private static string ParseName(string type)
		{
			return type.Split(":")[0].Trim();
		}
	}
}