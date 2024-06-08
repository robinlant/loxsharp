using loxsharp.Parser.Productions;

namespace loxsharp.Parser;

public interface ISyntaxTreeVisitor<T>
{
    T VisitBinary(Binary binary);

    T VisitGrouping(Grouping grouping);

    T VisitLiteral(Literal literal);

    T VisitUnary(Unary unary);
}