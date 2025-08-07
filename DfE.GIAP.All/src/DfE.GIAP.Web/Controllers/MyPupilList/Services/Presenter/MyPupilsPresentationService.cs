using System.Linq.Expressions;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Controllers.MyPupilList.FormState;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.SortExpressions;
using DfE.GIAP.Web.Controllers.MyPupilList.ViewModel;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.Presenter;

public sealed class MyPupilsPresentationService : IMyPupilsPresentationService
{
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _getMyPupilsUseCase;
    private readonly IMapper<PupilDto, PupilPresentatationModel> _mapper;
    private readonly ISortPupilsExpressionFactory _sortPupilsExpressionFactory;

    public MyPupilsPresentationService(
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> getMyPupilsUseCase,
        IMapper<PupilDto, PupilPresentatationModel> mapper,
        ISortPupilsExpressionFactory sortPupilsExpressionFactory)
    {
        _getMyPupilsUseCase = getMyPupilsUseCase;
        _mapper = mapper;
        _sortPupilsExpressionFactory = sortPupilsExpressionFactory;
    }

    public async Task<IEnumerable<PupilPresentatationModel>> GetPupilsForUserAsync(
        string userId,
        MyPupilsFormState state)
    {

        GetMyPupilsRequest request = new(userId);
        GetMyPupilsResponse response = await _getMyPupilsUseCase.HandleRequestAsync(request);


        List<PupilDto> sortedResultsByFormState = response.Pupils.ToList();

        if (!string.IsNullOrEmpty(state.SortBy))
        {
            Expression<Func<PupilDto, IComparable>> sortExpression = _sortPupilsExpressionFactory.Create(state.SortBy);

            sortedResultsByFormState =
                (state.SortDirection == SortDirection.Ascending ?
                    sortedResultsByFormState.AsQueryable().OrderBy(sortExpression) :
                    sortedResultsByFormState.AsQueryable().OrderByDescending(sortExpression))
                        .ToList();
        }

        // Page client-side
        const int DefaultPageSize = 20;
        int skip = DefaultPageSize * (state.Page.Value - 1);

        List<PupilPresentatationModel> pagedResults = sortedResultsByFormState
            .Skip(skip)
            .Take(DefaultPageSize)
            .Select(_mapper.Map)
            .ToList();

        return pagedResults;
    }
}
