using loxsharp.Parsing.Productions;

namespace loxsharp.Parsing;

public interface ISyntaxTreeVisitor<T>
{
    T VisitBinary(Binary binary);

    T VisitGrouping(Grouping grouping);

    T VisitLiteral(Literal literal);

    T VisitUnary(Unary unary);
}