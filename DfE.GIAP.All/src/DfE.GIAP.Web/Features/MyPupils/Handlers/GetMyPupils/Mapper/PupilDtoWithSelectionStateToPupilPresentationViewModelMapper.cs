using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.ViewModels;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;

public sealed class PupilDtoWithSelectionStateToPupilPresentationViewModelMapper : IMapper<PupilDtoWithSelectionStateDecorator, PupilViewModel>
{
    public PupilViewModel Map(PupilDtoWithSelectionStateDecorator input)
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
            IsSelected = input.IsSelected
        };
    }
}
