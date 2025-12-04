using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupils.Mapper;
internal sealed class PupilsSelectionContextToMyPupilsPresentationModelMapper : IMapper<PupilsSelectionContext, MyPupilsPresentationModel>
{
    public MyPupilsPresentationModel Map(PupilsSelectionContext input)
    {
        ArgumentNullException.ThrowIfNull(input);

        IEnumerable<MyPupilsPupilPresentationModel> pupils = input.Pupils.Values.Select((dto) =>
        {
            MyPupilsPupilPresentationModel viewModel = new()
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

        return MyPupilsPresentationModel.Create(pupils);
    }
}
