using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPupils;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetPupils.Mapper;

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
        if (input is null)
        {
            return MyPupilsPresentationPupilModels.Create([]);
        }

        return
            MyPupilsPresentationPupilModels.Create(
                pupils: input.Values.Select(_mapper.Map));
    }
}
