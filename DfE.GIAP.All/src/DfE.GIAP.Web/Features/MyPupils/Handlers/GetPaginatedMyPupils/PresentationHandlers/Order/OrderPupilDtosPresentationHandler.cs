using System.Linq.Expressions;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers.Order;
public sealed class OrderPupilDtosPresentationHandler : IPupilDtosPresentationHandler
{
    private static readonly Dictionary<string, Expression<Func<PupilDto, IComparable>>> s_sortKeyToExpression = new()
        {
            { "forename", (t) => t.Forename },
            { "surname", (t) => t.Surname },
            { "dob", (t) => t.ParseDateOfBirth() },
            { "sex", (t) => t.Sex }
        };

    public PupilDtos Handle(
        PupilDtos pupils,
        MyPupilsPresentationState state)
    {
        if (string.IsNullOrEmpty(state.SortBy))
        {
            return pupils;
        }

        if (!s_sortKeyToExpression.TryGetValue(state.SortBy.ToLowerInvariant(), out Expression<Func<PupilDto, IComparable>> expression)
                || expression is null)
        {
            throw new ArgumentException($"Unable to find sortable expression for {state.SortBy}");
        }

        IEnumerable<PupilDto> outputPupils
            = state.SortDirection == SortDirection.Ascending ?
                    pupils.Pupils.AsQueryable().OrderBy(expression) :
                    pupils.Pupils.AsQueryable().OrderByDescending(expression);

        return PupilDtos.Create(outputPupils);
    }
}
