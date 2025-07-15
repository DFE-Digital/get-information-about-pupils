using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.AuthorisationContext;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.PupilIdentifierEncoder;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;
using DfE.GIAP.Core.MyPupils.Domain.Services.PupilAggregator;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application;
internal sealed class GetMyPupilsHandler : IGetMyPupilsHandler
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

    public Task<IEnumerable<MyPupil>> Get(MyPupilsAuthorisationContext context)
    {
        // TODO DomainService to fetch what urns are from the UserIdentifier?
        string[] urns = [""]; //context.userId; // Call DomainService to get Urns?

        IEnumerable<Pupil> pupils = _domainServicePupilAggregator.GetPupils(urns);

        IEnumerable<MaskPupilIdentifierRequest> requests = pupils.Select(p => new MaskPupilIdentifierRequest(context, p));

        IEnumerable<PupilWithMyPupilIdentifier> pupilItems = requests.Select((req) =>
        {
            ShouldMaskPupilIdentifier shouldMask = _handler.Evaluate(req);
            MyPupilIdentifier identifier = new(req.Pupil.Identifier, shouldMask, _encoder);
            PupilWithMyPupilIdentifier mappablePupil = new(req.Pupil, identifier);
            return mappablePupil;
        });

        IEnumerable<MyPupil> outputPupilItems = pupilItems.Select(_mapper.Map);

        return Task.FromResult(outputPupilItems);
    }
}
