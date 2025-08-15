using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Dto;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Response;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Mapper;

public sealed class MapPupilDtoWithSelectionStateDecoratorToPupilPresentationViewModelMapper : IMapper<PupilDtoWithPupilSelectionStateDto, PupilPresentatationViewModel>
{
    public PupilPresentatationViewModel Map(PupilDtoWithPupilSelectionStateDto input)
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
