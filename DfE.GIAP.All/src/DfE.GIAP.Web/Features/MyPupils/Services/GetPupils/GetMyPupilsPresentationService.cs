using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations.ClearPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations.GetPupilSelections;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.GetPupils;
public sealed class GetMyPupilsPresentationService : IGetMyPupilsPresentationService
{
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _getMyPupilsUseCase;
    private readonly IGetMyPupilsPupilSelectionProvider _getMyPupilsStateProvider;
    private readonly IMapper<MyPupilsModels, MyPupilsPresentationPupilModels> _mapPupilsToPresentablePupils;

    public GetMyPupilsPresentationService(
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> deletePupilsUseCase,
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> getMyPupilsUseCase,
        IEvaluatorV2<MyPupilsPresentationHandlerRequest, MyPupilsPresentationPupilModels> evaluator,
        IGetMyPupilsPupilSelectionProvider getMyPupilsStateProvider,
        IClearMyPupilsPupilSelectionsHandler clearMyPupilsPupilSelectionsCommandHandler,
        IMapper<MyPupilsModels, MyPupilsPresentationPupilModels> mapper)
    {
        ArgumentNullException.ThrowIfNull(getMyPupilsUseCase);
        _getMyPupilsUseCase = getMyPupilsUseCase;

        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsStateProvider = getMyPupilsStateProvider;

        ArgumentNullException.ThrowIfNull(mapper);
        _mapPupilsToPresentablePupils = mapper;
    }

    public async Task<MyPupilsPresentationResponse> GetPupilsAsync(
        string userId,
        MyPupilsQueryRequestDto query)
    {
        UserId id = new(userId);
        query ??= new();

        MyPupilsPupilSelectionState selectionState =
            _getMyPupilsStateProvider.GetPupilSelections() ?? MyPupilsPupilSelectionState.CreateDefault();

        GetMyPupilsResponse response =
            await _getMyPupilsUseCase.HandleRequestAsync(
                new GetMyPupilsRequest(id.Value));

        MyPupilsPresentationQueryModel updatedPresentation = new(
            pageNumber: query.PageNumber,
            pageSize: query.PageSize,
            sortBy: query.SortField,
            sortDirection: query.SortDirection);

        MyPupilsPresentationPupilModels mappedPupils =
            _mapPupilsToPresentablePupils.Map(response.MyPupils) ?? MyPupilsPresentationPupilModels.Create([]);

        //Note: Mutates pupil
        foreach (MyPupilsPresentationPupilModel pupil in mappedPupils.Values)
        {
            pupil.IsSelected = selectionState.IsPupilSelected(pupil.UniquePupilNumber);
        }

        return new MyPupilsPresentationResponse()
        {
            MyPupils = mappedPupils,
            PageNumber = updatedPresentation.Page.Value,
            SortedDirection = updatedPresentation.Sort.Direction switch
            {
                SortDirection.Ascending => "asc",
                SortDirection.Descending => "desc",
                _ => string.Empty
            },
            SortedField = updatedPresentation.Sort.Field,
            IsAnyPupilsSelected = selectionState.IsAnyPupilSelected,
            TotalPages = CalulateTotalPages(response.MyPupils, updatedPresentation.PageSize)
        };
    }

    private static int CalulateTotalPages(MyPupilsModels pupils, int pageSize) =>
        (pupils == null) ||
            pupils.Count <= pageSize ?
                1 :
                    (int)Math.Ceiling(pupils.Count / (double)pageSize);
}
