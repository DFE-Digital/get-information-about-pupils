namespace DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;

public readonly struct ShouldMaskPupilIdentifier
{
    private ShouldMaskPupilIdentifier(bool isMasked)
    {
        ShouldMask = isMasked;
    }

    internal bool ShouldMask { get; }
    internal static ShouldMaskPupilIdentifier Mask => new(isMasked: true);
    internal static ShouldMaskPupilIdentifier DoNotMask => new(isMasked: true);
}
