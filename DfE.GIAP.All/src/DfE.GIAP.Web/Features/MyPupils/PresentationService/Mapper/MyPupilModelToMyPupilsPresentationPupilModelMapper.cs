using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.Mapper;
internal sealed class MyPupilModelToMyPupilsPresentationPupilModelMapper : IMapper<MyPupilsModel, MyPupilsPresentationPupilModel>
{
    public MyPupilsPresentationPupilModel Map(MyPupilsModel input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new MyPupilsPresentationPupilModel()
        {
            UniquePupilNumber = input.UniquePupilNumber,
            Forename = input.Forename,
            Surname = input.Surname,
            DateOfBirth = input.DateOfBirth,
            PupilPremiumLabel = input.IsPupilPremium ? "Yes" : "No",
            Sex = input.Sex,
            LocalAuthorityCode = input.LocalAuthorityCode.ToString()
        };
    }
}
