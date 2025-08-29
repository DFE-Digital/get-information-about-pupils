
namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
public sealed class PupilDtos
{
    public PupilDtos(IEnumerable<PupilDto> pupilDtos)
    {
        Pupils =
            (pupilDtos is null ? [] : pupilDtos)
                .ToList()
                .AsReadOnly();
    }

    public static PupilDtos Empty() => Create([]);
    public static PupilDtos Create(IEnumerable<PupilDto> pupils) => new(pupils);

    public IReadOnlyList<PupilDto> Pupils { get; }
    public int Count => Pupils.Count;
    public bool IsEmpty() => !Pupils.Any();
}
