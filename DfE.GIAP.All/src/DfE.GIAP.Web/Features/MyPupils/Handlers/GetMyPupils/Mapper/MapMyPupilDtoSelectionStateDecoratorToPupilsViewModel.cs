using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;

internal sealed class MapMyPupilDtoSelectionStateDecoratorToPupilsViewModel : IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>
{
    public PupilsViewModel Map(MyPupilsDtoSelectionStateDecorator input)
    {
        ArgumentNullException.ThrowIfNull(input);

        IEnumerable<PupilViewModel> pupils = input.MyPupilDtos.Values.Select((dto) =>
        {
            PupilViewModel viewModel = new()
            {
                UniquePupilNumber = dto.UniquePupilNumber,
                Forename = dto.Forename,
                Surname = dto.Surname,
                DateOfBirth = dto.DateOfBirth,
                PupilPremiumLabel = dto.IsPupilPremium ? "Yes" : "No",
                Sex = dto.Sex,
                LocalAuthorityCode = dto.LocalAuthorityCode.ToString(),
                IsSelected = input.SelectionState.IsPupilSelected(dto.UniquePupilNumber)
            };
            return viewModel;
        });

        return PupilsViewModel.Create(pupils);
    }
}
