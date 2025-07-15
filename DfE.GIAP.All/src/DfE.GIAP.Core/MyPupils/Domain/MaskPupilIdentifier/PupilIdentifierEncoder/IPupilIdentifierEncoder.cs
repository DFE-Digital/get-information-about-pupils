namespace DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.PupilIdentifierEncoder;
public interface IPupilIdentifierEncoder
{
    string Encode(UniquePupilIdentifier identifier);
}

// TODO pull in default impl
