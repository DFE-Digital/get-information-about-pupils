using DfE.GIAP.Web.Features.MyPupils.State;

namespace DfE.GIAP.Web.Features.MyPupils.GetPupilViewModels;

public record GetPupilViewModelsRequest(
    string UserId,
    MyPupilsState State);
