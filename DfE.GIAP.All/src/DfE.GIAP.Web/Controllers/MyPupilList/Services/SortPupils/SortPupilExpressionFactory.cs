using System.Globalization;
using System.Linq.Expressions;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.SortExpressions;

public sealed class SortPupilsExpressionFactory : ISortPupilsExpressionFactory
{
    private static readonly Dictionary<string, Expression<Func<PupilDto, IComparable>>> sortKeyToExpression = new()
        {
            { "Forename", (t) => t.Forename },
            { "Surname", (t) => t.Surname },
            { "DOB", (t) => DateTime.Parse(t.DateOfBirth, new CultureInfo("en-GB")) },
            { "Sex", (t) => t.Sex }
        };

    public Expression<Func<PupilDto, IComparable>> Create(string sortKey)
    {
        if (!sortKeyToExpression.TryGetValue(sortKey, out Expression<Func<PupilDto, IComparable>> expression) || expression is null)
        {
            throw new ArgumentException($"Unable to find expression for {sortKey}");
        }
        return expression;
    }
}
