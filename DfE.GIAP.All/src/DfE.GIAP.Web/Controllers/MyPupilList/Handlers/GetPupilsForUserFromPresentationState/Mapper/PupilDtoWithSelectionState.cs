using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetPupilsForUserFromPresentationState.Mapper;

public record PupilDtoWithSelectionState(PupilDto Pupil, bool isSelected);
