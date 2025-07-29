using DfE.GIAP.Core.MyPupils.Domain;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
public record GetMyPupilsResponse(IEnumerable<PupilDto> Pupils);
