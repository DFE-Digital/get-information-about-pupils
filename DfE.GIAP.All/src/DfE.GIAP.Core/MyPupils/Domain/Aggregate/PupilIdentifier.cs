using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.Aggregate;
public record PupilIdentifier(PupilId PupilId, UniquePupilNumber UniquePupilNumber);
