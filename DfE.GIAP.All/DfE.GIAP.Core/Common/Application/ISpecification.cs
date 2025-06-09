using System.Linq.Expressions;

namespace DfE.GIAP.Core.Common.Application;

public interface IFilterSpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
    bool IsSatisfiedBy(T input);
}
