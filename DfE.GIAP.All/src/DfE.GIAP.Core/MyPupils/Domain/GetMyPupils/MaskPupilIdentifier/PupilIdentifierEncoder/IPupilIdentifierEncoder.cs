namespace DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.PupilIdentifierEncoder;
public interface IPupilIdentifierEncoder
{
    string Encode(UniquePupilIdentifier identifier);
}

// TODO pull in default impl
