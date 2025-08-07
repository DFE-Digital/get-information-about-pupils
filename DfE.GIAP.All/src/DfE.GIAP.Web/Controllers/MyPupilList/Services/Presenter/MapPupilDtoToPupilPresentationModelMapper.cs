using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Controllers.MyPupilList.ViewModel;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.Presenter;

public class MapPupilDtoToPupilPresentationModelMapper : IMapper<PupilDto, PupilPresentatationModel>
{
    public PupilPresentatationModel Map(PupilDto input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new(input);
    }
}
