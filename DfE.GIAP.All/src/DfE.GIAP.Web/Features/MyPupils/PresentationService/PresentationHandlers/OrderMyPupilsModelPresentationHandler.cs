using System.Linq.Expressions;
using DfE.GIAP.Web.Features.MyPupils.State.Models;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;

public sealed class OrderMyPupilsModelPresentationHandler : IMyPupilsPresentationModelHandler
{
    private static readonly Dictionary<string, Expression<Func<MyPupilsPresentationPupilModel, IComparable>>> s_sortKeyToExpression = new()
    {
        { "forename", (t) => t.Forename },
        { "surname", (t) => t.Surname },
        { "dob", (t) => t.ParseDateOfBirth() },
        { "sex", (t) => t.Sex }
    };

    public MyPupilsPresentationPupilModels Handle(
        MyPupilsPresentationPupilModels myPupils,
        MyPupilsState state)
    {
        if (string.IsNullOrEmpty(state.PresentationState.SortBy))
        {
            return myPupils;
        }

        if (!s_sortKeyToExpression.TryGetValue(state.PresentationState.SortBy.ToLowerInvariant(),
                out Expression<Func<MyPupilsPresentationPupilModel, IComparable>> expression)
                    || expression is null)
        {
            throw new ArgumentException($"Unable to find sortable expression for {state.PresentationState.SortBy}");
        }

        IEnumerable<MyPupilsPresentationPupilModel> outputPupils
            = state.PresentationState.SortDirection == SortDirection.Ascending ?
                myPupils.Values.AsQueryable().OrderBy(expression) :
                    myPupils.Values.AsQueryable().OrderByDescending(expression);

        return MyPupilsPresentationPupilModels.Create(outputPupils);
    }
}
