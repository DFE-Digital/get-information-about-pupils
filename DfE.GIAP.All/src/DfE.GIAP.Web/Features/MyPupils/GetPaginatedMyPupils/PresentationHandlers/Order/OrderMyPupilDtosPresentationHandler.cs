using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using System.Linq.Expressions;

namespace DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils.PresentationHandlers.Order;

public sealed class OrderMyPupilDtosPresentationHandler : IMyPupilDtosPresentationHandler
{
    private static readonly Dictionary<string, Expression<Func<MyPupilModel, IComparable>>> s_sortKeyToExpression = new()
        {
            { "forename", (t) => t.Forename },
            { "surname", (t) => t.Surname },
            { "dob", (t) => t.ParseDateOfBirth() },
            { "sex", (t) => t.Sex }
        };

    public MyPupilsModel Handle(
        MyPupilsModel myPupils,
        MyPupilsPresentationState state)
    {
        if (string.IsNullOrEmpty(state.SortBy))
        {
            return myPupils;
        }

        if (!s_sortKeyToExpression.TryGetValue(state.SortBy.ToLowerInvariant(), out Expression<Func<MyPupilModel, IComparable>> expression)
                || expression is null)
        {
            throw new ArgumentException($"Unable to find sortable expression for {state.SortBy}");
        }

        IEnumerable<MyPupilModel> outputPupils
            = state.SortDirection == SortDirection.Ascending ?
                    myPupils.Values.AsQueryable().OrderBy(expression) :
                    myPupils.Values.AsQueryable().OrderByDescending(expression);

        return MyPupilsModel.Create(pupils: outputPupils);
    }
}
