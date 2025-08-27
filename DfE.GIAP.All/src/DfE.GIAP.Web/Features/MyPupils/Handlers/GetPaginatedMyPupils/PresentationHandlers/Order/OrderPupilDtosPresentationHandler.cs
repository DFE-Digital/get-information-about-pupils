using System.Linq.Expressions;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.PresentationState;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers.Order;
public sealed class OrderPupilDtosPresentationHandler : IPupilPresentationHandler
{
    private static readonly Dictionary<string, Expression<Func<PupilDto, IComparable>>> s_sortKeyToExpression = new()
        {
            { "forename", (t) => t.Forename },
            { "surname", (t) => t.Surname },
            { "dob", (t) => t.ParseDateOfBirth() },
            { "sex", (t) => t.Sex }
        };

    public IEnumerable<PupilDto> Handle(
        IEnumerable<PupilDto> pupils,
        MyPupilsPresentationState options)
    {
        if (string.IsNullOrEmpty(options.SortBy))
        {
            return pupils;
        }

        if (!s_sortKeyToExpression.TryGetValue(options.SortBy.ToLowerInvariant(), out Expression<Func<PupilDto, IComparable>> expression)
                || expression is null)
        {
            throw new ArgumentException($"Unable to find sortable expression for {options.SortBy}");
        }

        return options.SortDirection == SortDirection.Ascending ?
                    pupils.AsQueryable().OrderBy(expression) :
                    pupils.AsQueryable().OrderByDescending(expression);
    }
}
