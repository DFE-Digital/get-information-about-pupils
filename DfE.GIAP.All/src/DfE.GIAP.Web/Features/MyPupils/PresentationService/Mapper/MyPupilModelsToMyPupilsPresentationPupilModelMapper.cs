using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.Mapper;

public class MyPupilModelsToMyPupilsPresentationPupilModelMapper : IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>
{
    private readonly IMapper<MyPupilsModel, MyPupilsPresentationPupilModel> _mapper;

    public MyPupilModelsToMyPupilsPresentationPupilModelMapper(IMapper<MyPupilsModel, MyPupilsPresentationPupilModel> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        _mapper = mapper;
    }
    public MyPupilsPresentationPupilModels Map(MyPupilsModels input)
    {
        return
            MyPupilsPresentationPupilModels.Create(
                pupils: input.Values.Select(_mapper.Map));
    } 
}
