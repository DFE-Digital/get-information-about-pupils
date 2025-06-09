using System.Linq.Expressions;

namespace DfE.GIAP.Core.Common.Application;
public sealed class AndSpecificaton<T> : ISpecification<T>
{
    public AndSpecificaton(ISpecification<T> left, ISpecification<T> right)
    {
        Left = left;
        Right = right;
    }

    public bool IsSatisfiedBy(T input) => Left.IsSatisfiedBy(input) && Right.IsSatisfiedBy(input);

    public ISpecification<T> Left { get; }
    public ISpecification<T> Right { get; }

    public Expression<Func<T, bool>> ToExpression()
    {
        Expression<Func<T, bool>> leftExpr = Left.ToExpression();
        Expression<Func<T, bool>> rightExpr = Right.ToExpression();

        // Required to replace parameter to include the original expression else LINQ-to-SQL providers like EFCore complain, as the expression "member" is different so inject one in.
        ParameterExpression parameter = Expression.Parameter(type: typeof(T), name: "x");

        Expression leftBody = new ReplaceParameterVisitor(leftExpr.Parameters[0], parameter).Visit(leftExpr.Body);
        Expression rightBody = new ReplaceParameterVisitor(rightExpr.Parameters[0], parameter).Visit(rightExpr.Body);

        BinaryExpression body = Expression.AndAlso(leftBody!, rightBody!);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private sealed class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParam;
        private readonly ParameterExpression _newParam;

        public ReplaceParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam)
        {
            _oldParam = oldParam;
            _newParam = newParam;
        }

        protected override Expression VisitParameter(ParameterExpression node) => node == _oldParam ? _newParam : base.VisitParameter(node);
    }
}
