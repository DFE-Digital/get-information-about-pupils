using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.GetPupils;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;

public sealed class PaginateMyPupilsModelPresentationHandler : IEvaluationHandlerV2<MyPupilsPresentationHandlerRequest, MyPupilsPresentationPupilModels>
{
    public ValueTask<HandlerResult<MyPupilsPresentationPupilModels>> HandleAsync(
        MyPupilsPresentationHandlerRequest input,
        CancellationToken ctx = default)
    {
        if (input is null)
        {
            return HandlerResultValueTaskFactory.FailedWithNullArgument<MyPupilsPresentationPupilModels>(nameof(input));
        }

        const int DefaultPageSize = 20;

        int skip = DefaultPageSize * (input.Query.Page.Value - 1);

        if (skip >= input.Pupils.Count)
        {
            return HandlerResultValueTaskFactory.Success(
                MyPupilsPresentationPupilModels.Empty());
        }

        List<MyPupilsPresentationPupilModel> pagedResults = input.Pupils.Values
            .Skip(skip)
            .Take(DefaultPageSize)
            .ToList();

        return HandlerResultValueTaskFactory.Success(
            MyPupilsPresentationPupilModels.Create(pagedResults));
    }
}
