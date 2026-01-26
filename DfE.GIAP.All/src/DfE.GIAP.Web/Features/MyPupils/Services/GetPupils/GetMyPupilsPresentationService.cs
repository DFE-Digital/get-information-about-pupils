using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.GetPupilSelections;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.GetPupils;
public sealed class GetMyPupilsPresentationService : IGetMyPupilsPresentationService
{
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _getMyPupilsUseCase;
    private readonly IGetMyPupilsPupilSelectionProvider _getMyPupilsStateProvider;
    private readonly IMapper<MyPupilsModels, MyPupilsPresentationPupilModels> _mapPupilsToPresentablePupils;

    public GetMyPupilsPresentationService(
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> getMyPupilsUseCase,
        IGetMyPupilsPupilSelectionProvider getMyPupilsStateProvider,
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

        MyPupilsQueryModel updatedQueryModel = new(
            pageNumber: query.PageNumber,
            size: query.PageSize,
            orderBy: (query.SortField, query.SortDirection));

        GetMyPupilsResponse response =
            await _getMyPupilsUseCase.HandleRequestAsync(
                new GetMyPupilsRequest(
                    userId: id.Value,
                    updatedQueryModel));

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
            PageNumber = updatedQueryModel.PaginateOptions.Page.Value,
            SortedDirection = updatedQueryModel.Order.Direction switch
            {
                OrderDirection.Ascending => "asc",
                OrderDirection.Descending => "desc",
                _ => string.Empty
            },
            SortedField = updatedQueryModel.Order.Field,
            IsAnyPupilsSelected = selectionState.IsAnyPupilSelected,
        };
    }
}
