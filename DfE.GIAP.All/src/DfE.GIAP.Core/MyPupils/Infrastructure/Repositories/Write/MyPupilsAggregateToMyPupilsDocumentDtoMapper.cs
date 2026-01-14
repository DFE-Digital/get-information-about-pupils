namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Write;
internal sealed class MyPupilsAggregateToMyPupilsDocumentDtoMapper : IMapper<MyPupilsAggregate, MyPupilsDocumentDto>
{
    public MyPupilsDocumentDto Map(MyPupilsAggregate input)
    {
        ArgumentNullException.ThrowIfNull(input);

        IEnumerable<MyPupilsPupilDto> updatedPupils = input.GetMyPupils()?.Select((upn) => new MyPupilsPupilDto()
        {
            UPN = upn.Value
        }) ?? [];

        MyPupilsDto pupilsDto = new()
        {
            Pupils = updatedPupils,
        };

        MyPupilsDocumentDto documentDto = new()
        {
            id = input.AggregateId.Value,
            MyPupils = pupilsDto
        };

        return documentDto;
    }
}
