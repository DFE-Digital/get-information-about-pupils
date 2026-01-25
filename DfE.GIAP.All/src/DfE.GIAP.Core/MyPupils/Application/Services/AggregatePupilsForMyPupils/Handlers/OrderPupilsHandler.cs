using System.Linq.Expressions;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Handlers;
internal sealed class OrderPupilsHandler : IOrderPupilsHandler
{
    private static readonly Dictionary<string, Expression<Func<Pupil, IComparable>>> _sortKeyToExpression = new()
    {
        { "forename", (t) => t.Forename },
        { "surname", (t) => t.Surname },
        { "dob", (t) => t.TryParseDateOfBirth() ?? DateTime.MinValue },
        { "sex", (t) => t.Sex }
    };

    public IEnumerable<Pupil> Order(IEnumerable<Pupil> pupils, OrderOptions options)
    {
        if (pupils is null)
        {
            return [];
        }

        if (pupils.TryGetNonEnumeratedCount(out int count) && count == 0)
        {
            return [];
        }

        if (options is null)
        {
            return pupils;
        }

        if (string.IsNullOrWhiteSpace(options.Field))
        {
            return pupils;
        }

        if (!_sortKeyToExpression.TryGetValue(options.Field.ToLowerInvariant(),
                  out Expression<Func<Pupil, IComparable>>? sortExpression)
                        || sortExpression is null)
        {
            throw new ArgumentException($"Unable to find sortable expression for {options.Field}");
        }

        return options.Direction == OrderDirection.Descending ?
            pupils.AsQueryable().OrderByDescending(sortExpression) :
            pupils.AsQueryable().OrderBy(sortExpression);
    }
}
