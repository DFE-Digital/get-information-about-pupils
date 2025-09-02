using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel.Factory;

internal sealed class PupilViewModelFactory : IPupilsViewModelFactory
{
    private readonly IMapper<MyPupilDto, PupilViewModel> _mapMyPupilDtoToPupilViewModel;

    public PupilViewModelFactory(IMapper<MyPupilDto, PupilViewModel> mapMyPupilDtoToPupilViewModel)
    {
        ArgumentNullException.ThrowIfNull(mapMyPupilDtoToPupilViewModel);
        _mapMyPupilDtoToPupilViewModel = mapMyPupilDtoToPupilViewModel;
    }

    public PupilsViewModel CreateViewModel(MyPupilDtos myPupils, MyPupilsPupilSelectionState selectionState)
    {
        IEnumerable<PupilViewModel> pupils = myPupils.Values.Select(dto =>
        {
            bool IsPupilSelected = selectionState.IsPupilSelected(dto.UniquePupilNumber);
            PupilViewModel mappedViewModel = _mapMyPupilDtoToPupilViewModel.Map(dto);
            return mappedViewModel with { IsSelected = IsPupilSelected };
        });

        return PupilsViewModel.Create(pupils);
    }
}
