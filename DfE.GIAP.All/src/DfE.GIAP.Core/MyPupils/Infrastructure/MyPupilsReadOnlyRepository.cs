using DfE.GIAP.Core.MyPupils.Application.Repository;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.PupilIdentifierEncoder;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Pupil.Domain;

namespace DfE.GIAP.Core.MyPupils.Infrastructure;
internal sealed class MyPupilReadOnlyRepository : IMyPupilReadOnlyRepository
{
    private readonly IPupilIdentifierEncoder _encoder;
    private readonly IPupilAggregatorService _pupilAggregatorService;

    public MyPupilReadOnlyRepository(
        IPupilAggregatorService pupilAggregatorService,
        IPupilIdentifierEncoder encoder)
    {
        _pupilAggregatorService = pupilAggregatorService;
        _encoder = encoder;
    }

    public async Task<IEnumerable<MyPupil>> GetMyPupilsById(
        IEnumerable<UniquePupilIdentifier> upns,
        MyPupilsAuthorisationContext context,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Pupil.Domain.Pupil> pupils = await _pupilAggregatorService.GetPupils(upns, cancellationToken);
        IEnumerable<MyPupil> myPupils = MapToMyPupils(pupils, context);
        return myPupils;
    }

    private IEnumerable<MyPupil> MapToMyPupils(IEnumerable<Pupil.Domain.Pupil> pupils, MyPupilsAuthorisationContext myPupilsAuthorisationContext)
    {
        IEnumerable<MyPupil> outputPupilItems =
            pupils.Select((pupil) =>
            {
                IPupilIdentifierEncoder encoder = myPupilsAuthorisationContext.ShouldMaskPupil(pupil) ? _encoder : new NoOperationEncoder();
                MyPupilIdentifier myPupiId = new(pupil.Identifier, encoder);
                return new MyPupil(myPupiId, pupil);
            });

        return outputPupilItems;
    }

    private sealed class NoOperationEncoder : IPupilIdentifierEncoder
    {
        public string Encode(UniquePupilIdentifier identifier) => identifier.Value.ToString();
    }
}
