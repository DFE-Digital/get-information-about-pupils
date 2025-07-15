using DfE.GIAP.Core.Common.Domain.Pupil;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public record PupilWithMyPupilIdentifier(Pupil pupil, MyPupilIdentifier identifier);
