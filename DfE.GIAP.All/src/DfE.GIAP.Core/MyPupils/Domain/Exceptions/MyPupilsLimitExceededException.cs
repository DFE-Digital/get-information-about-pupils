namespace DfE.GIAP.Core.MyPupils.Domain.Exceptions;
public sealed class MyPupilsLimitExceededException : Exception
{
    public MyPupilsLimitExceededException(int maxAllowed)
        : base($"Cannot add more pupils to MyPupils. Limit of {maxAllowed}.")
    {
    }
}
