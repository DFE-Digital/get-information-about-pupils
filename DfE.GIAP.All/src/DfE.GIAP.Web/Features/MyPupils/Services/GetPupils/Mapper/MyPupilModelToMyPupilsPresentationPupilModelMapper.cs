using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPupils;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetPupils.Mapper;
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
            LocalAuthorityCode = input.LocalAuthorityCode?.ToString() ?? string.Empty
        };
    }
}
