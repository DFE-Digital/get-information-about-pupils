using DfE.GIAP.Core.MyPupils.Domain.Aggregate;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
public record GetMyPupilsResponse(IEnumerable<PupilDto> Pupils);
