using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
using DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.PupilIdentifierEncoder;
using DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.Rules.Abstraction;
using DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.ValueObjects;
using DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.ValueObjects;
using DfE.GIAP.Core.MyPupils.Domain.Services.PupilAggregator;

namespace DfE.GIAP.Core.MyPupils.Domain.GetMyPupils;
internal sealed class GetMyPupilsHandler
{
    private readonly IChainEvaluationHandler<MaskPupilIdentifierRequest, ShouldMaskPupilIdentifier> _handler;
    private readonly IMapper<PupilWithMyPupilIdentifier, MyPupil> _mapper;
    private readonly IPupilAggregatorService _domainServicePupilAggregator;
    private readonly IPupilIdentifierEncoder _encoder;

    public GetMyPupilsHandler(
        IMapper<PupilWithMyPupilIdentifier, MyPupil> mapper,
        IPupilAggregatorService domainServiceForPupilData,
        IPupilIdentifierEncoder encoder,
        IChainEvaluationHandler<MaskPupilIdentifierRequest, ShouldMaskPupilIdentifier> handler)
    {
        _mapper = mapper;
        _domainServicePupilAggregator = domainServiceForPupilData;
        _encoder = encoder;
        _handler = handler;
    }

    public MyPupilsResponse Get(string[] upns, MyPupilsAuthorisationContext context)
    {

        IEnumerable<Pupil> pupils = _domainServicePupilAggregator.GetPupils(upns);
        IEnumerable<MaskPupilIdentifierRequest> requests = pupils.Select(p => new MaskPupilIdentifierRequest(context, p));

        IEnumerable<MyPupil> pupilItems = requests.Select(req =>
        {
            ShouldMaskPupilIdentifier shouldMask = _handler.Evaluate(req);

            MyPupilIdentifier identifier = new(
                req.Pupil.Id,
                shouldMask,
                _encoder);

            PupilWithMyPupilIdentifier mappablePupil = new(req.Pupil, identifier);
            return _mapper.Map(mappablePupil);
        });

        return new MyPupilsResponse(pupilItems);
    }
}
