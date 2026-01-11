using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection;
using System.Linq.Expressions;

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
        MyPupilsPresentationQueryModel query,
        MyPupilsPupilSelectionState _)
    {
        if (string.IsNullOrEmpty(query.Sort.Field))
        {
            return myPupils;
        }

        if (!s_sortKeyToExpression.TryGetValue(query.Sort.Field.ToLowerInvariant(),
                out Expression<Func<MyPupilsPresentationPupilModel, IComparable>> expression)
                    || expression is null)
        {
            throw new ArgumentException($"Unable to find sortable expression for {query.Sort.Field}");
        }

        IEnumerable<MyPupilsPresentationPupilModel> outputPupils =
            query.Sort.Direction == SortDirection.Ascending ?
                myPupils.Values.AsQueryable().OrderBy(expression) :
                    myPupils.Values.AsQueryable().OrderByDescending(expression);

        return MyPupilsPresentationPupilModels.Create(outputPupils);
    }
}