using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Dto;

public record PupilDtoWithPupilSelectionStateDto(PupilDto Pupil, bool isSelected);
