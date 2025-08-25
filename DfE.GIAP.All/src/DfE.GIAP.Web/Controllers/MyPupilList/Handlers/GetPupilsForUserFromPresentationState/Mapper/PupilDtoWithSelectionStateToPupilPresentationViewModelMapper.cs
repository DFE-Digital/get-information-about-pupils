using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetPupilsForUserFromPresentationState.Response;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetPupilsForUserFromPresentationState.Mapper;

public sealed class PupilDtoWithSelectionStateToPupilPresentationViewModelMapper : IMapper<PupilDtoWithSelectionState, PupilPresentationViewModel>
{
    public PupilPresentationViewModel Map(PupilDtoWithSelectionState input)
    {
        return new()
        {
            UniquePupilNumber = input.Pupil.UniquePupilNumber,
            Forename = input.Pupil.Forename,
            Surname = input.Pupil.Surname,
            DateOfBirth = input.Pupil.DateOfBirth,
            PupilPremiumLabel = input.Pupil.IsPupilPremium ? "Yes" : "No",
            Sex = input.Pupil.Sex,
            LocalAuthorityCode = input.Pupil.LocalAuthorityCode.ToString(),
            IsSelected = input.isSelected
        };
    }
}
