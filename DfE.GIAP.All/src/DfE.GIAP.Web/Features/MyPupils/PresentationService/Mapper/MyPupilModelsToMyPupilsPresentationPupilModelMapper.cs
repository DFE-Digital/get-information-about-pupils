using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.Mapper;

public class MyPupilModelsToMyPupilsPresentationPupilModelMapper : IMapper<MyPupilsModel, MyPupilsPresentationPupilModels>
{
    private readonly IMapper<MyPupilModel, MyPupilsPresentationPupilModel> _mapper;

    public MyPupilModelsToMyPupilsPresentationPupilModelMapper(IMapper<MyPupilModel, MyPupilsPresentationPupilModel> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        _mapper = mapper;
    }
    public MyPupilsPresentationPupilModels Map(MyPupilsModel input)
    {
        return
            MyPupilsPresentationPupilModels.Create(
                pupils: input.Values.Select(_mapper.Map));
    } 
}
