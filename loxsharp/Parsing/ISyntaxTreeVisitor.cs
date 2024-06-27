using loxsharp.Parsing.Productions;

namespace loxsharp.Parsing;

public interface ISyntaxTreeVisitor<out T>
{
    T VisitAssign(Assign assign);

    T VisitBinary(Binary binary);

    T VisitCall(Call call);

    T VisitGrouping(Grouping grouping);

    T VisitLiteral(Literal literal);

    T VisitLogical(Logical logical);

    T VisitUnary(Unary unary);

    T VisitConditional(Conditional conditional);

    T VisitVariable(Variable variable);
}