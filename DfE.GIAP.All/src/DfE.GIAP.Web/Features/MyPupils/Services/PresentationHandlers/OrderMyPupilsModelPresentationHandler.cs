using System.Linq.Expressions;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.GetPupils;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;

public sealed class OrderMyPupilsModelPresentationHandler : IEvaluationHandlerV2<MyPupilsPresentationHandlerRequest, MyPupilsPresentationPupilModels>
{
    private static readonly Dictionary<string, Expression<Func<MyPupilsPresentationPupilModel, IComparable>>> _sortKeyToExpression = new()
    {
        { "forename", (t) => t.Forename },
        { "surname", (t) => t.Surname },
        { "dob", (t) => t.ParseDateOfBirth() },
        { "sex", (t) => t.Sex }
    };


    public ValueTask<HandlerResult<MyPupilsPresentationPupilModels>> HandleAsync(
        MyPupilsPresentationHandlerRequest input,
        CancellationToken ctx = default)
    {
        if(input is null)
        {
            return HandlerResultValueTaskFactory.FailedWithNullArgument<MyPupilsPresentationPupilModels>(nameof(input));
        }

        // no pupils to sort
        if (input.Pupils.Count == 0)
        {
            return HandlerResultValueTaskFactory.Success(
                MyPupilsPresentationPupilModels.Create([]));
        }

        // no sort to apply
        if (string.IsNullOrEmpty(input.Query.Sort.Field))
        {
            return HandlerResultValueTaskFactory.Success(input.Pupils);
        }

        if (!_sortKeyToExpression.TryGetValue(input.Query.Sort.Field.ToLowerInvariant(),
                out Expression<Func<MyPupilsPresentationPupilModel, IComparable>> expression)
                    || expression is null)
        {
            return HandlerResultValueTaskFactory.Failed<MyPupilsPresentationPupilModels>(
                new ArgumentException($"Unable to find sortable expression for {input.Query.Sort.Field}"));
        }

        IEnumerable<MyPupilsPresentationPupilModel> outputPupils =
            input.Query.Sort.Direction == SortDirection.Ascending ?
                input.Pupils.Values.AsQueryable().OrderBy(expression) :
                    input.Pupils.Values.AsQueryable().OrderByDescending(expression);

        return
            HandlerResultValueTaskFactory.Success(
                MyPupilsPresentationPupilModels.Create(outputPupils));
    }
}
