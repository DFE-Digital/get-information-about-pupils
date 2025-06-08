using System.Linq.Expressions;
using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles.FilterFactory.Specification;
public sealed class AndSpecificaton<T> : IFilterSpecification<T>
{
    private readonly IFilterSpecification<T> _left;
    private readonly IFilterSpecification<T> _right;

    public AndSpecificaton(IFilterSpecification<T> left, IFilterSpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public bool IsSatisfiedBy(T input) => _left.IsSatisfiedBy(input) && _right.IsSatisfiedBy(input);

    public Expression<Func<T, bool>> ToExpression()
    {
        Expression<Func<T, bool>> leftExpr = _left.ToExpression();
        Expression<Func<T, bool>> rightExpr = _right.ToExpression();

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


    public string ToFilterQuery(string alias = "c") => $"{_left.ToFilterQuery(alias)} AND {_right.ToFilterQuery(alias)}";
}
