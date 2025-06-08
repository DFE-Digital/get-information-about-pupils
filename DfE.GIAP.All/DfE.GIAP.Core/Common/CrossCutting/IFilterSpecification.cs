using System.Linq.Expressions;

namespace DfE.GIAP.Core.Common.CrossCutting;

public interface IFilterSpecification<T>
{
    string ToFilterQuery(string alias);
    Expression<Func<T, bool>> ToExpression();
    bool IsSatisfiedBy(T input);
}
