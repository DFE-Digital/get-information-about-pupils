using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.Domain.Pupil;
using DfE.GIAP.Core.MyPupils.Application.UseCase.GetMyPupils.PupilAggregatorService;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.PupilIdentifierEncoder;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCase.GetMyPupils.Handler;
internal sealed class GetMyPupilsHandler : IGetMyPupilsHandler
{
    private readonly IPupilIdentifierEncoder _encoder;
    private readonly IPupilAggregatorService _pupilAggregatorService;

    public GetMyPupilsHandler(
        IPupilAggregatorService pupilAggregatorService,
        IPupilIdentifierEncoder encoder)
    {
        _pupilAggregatorService = pupilAggregatorService;
        _encoder = encoder;
    }

    public async Task<IEnumerable<MyPupil>> HandleAsync(
        MyPupilsAuthorisationContext context,
        IEnumerable<string> urns,
        CancellationToken ctx = default)
    {
        IEnumerable<Pupil> pupils = await _pupilAggregatorService.GetPupils(urns, ctx);

        IEnumerable<MyPupil> outputPupilItems = pupils
            .Select(pupil =>
            {
                IPupilIdentifierEncoder encoder = context.ShouldMaskPupil(pupil) ? _encoder : new NoOperationEncoder();
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

