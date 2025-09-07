using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using System.Linq.Expressions;

namespace DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils.PresentationHandlers.Order;

public sealed class OrderMyPupilDtosPresentationHandler : IMyPupilDtosPresentationHandler
{
    private static readonly Dictionary<string, Expression<Func<MyPupilDto, IComparable>>> s_sortKeyToExpression = new()
        {
            { "forename", (t) => t.Forename },
            { "surname", (t) => t.Surname },
            { "dob", (t) => t.ParseDateOfBirth() },
            { "sex", (t) => t.Sex }
        };

    public MyPupilDtos Handle(
        MyPupilDtos myPupils,
        MyPupilsPresentationState state)
    {
        if (string.IsNullOrEmpty(state.SortBy))
        {
            return myPupils;
        }

        if (!s_sortKeyToExpression.TryGetValue(state.SortBy.ToLowerInvariant(), out Expression<Func<MyPupilDto, IComparable>> expression)
                || expression is null)
        {
            throw new ArgumentException($"Unable to find sortable expression for {state.SortBy}");
        }

        IEnumerable<MyPupilDto> outputPupils
            = state.SortDirection == SortDirection.Ascending ?
                    myPupils.Values.AsQueryable().OrderBy(expression) :
                    myPupils.Values.AsQueryable().OrderByDescending(expression);

        return MyPupilDtos.Create(pupils: outputPupils);
    }
}
