using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;

public record MyPupilDtoWithSelectionStateDecorator(MyPupilDto Pupil, bool IsSelected);
