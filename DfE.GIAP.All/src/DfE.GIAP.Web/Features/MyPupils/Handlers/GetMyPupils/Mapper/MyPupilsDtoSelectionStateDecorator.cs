using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;

public record MyPupilsDtoSelectionStateDecorator(MyPupilDtos MyPupilDtos, MyPupilsPupilSelectionState SelectionState);
