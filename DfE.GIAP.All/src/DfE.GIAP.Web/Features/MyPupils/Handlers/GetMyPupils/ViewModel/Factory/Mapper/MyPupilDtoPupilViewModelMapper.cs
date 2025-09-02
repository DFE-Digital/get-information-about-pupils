using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel.Factory.Mapper;

internal sealed class MyPupilDtoPupilViewModelMapper : IMapper<MyPupilDto, PupilViewModel>
{
    public PupilViewModel Map(MyPupilDto input)
    {
        return new()
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
