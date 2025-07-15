using DfE.GIAP.Core.Pupil.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain.PupilIdentifierEncoder;
public interface IPupilIdentifierEncoder
{
    string Encode(UniquePupilIdentifier identifier);
}

// TODO pull in default impl
