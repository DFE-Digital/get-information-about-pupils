using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Read.Mapper;
internal sealed class MyPupilsDocumentDtoToMyPupilsMapper : IMapper<MyPupilsDocumentDto, Application.Repositories.MyPupils>
{
    public Application.Repositories.MyPupils Map(MyPupilsDocumentDto input)
    {
        ArgumentNullException.ThrowIfNull(input);

        UniquePupilNumbers uniquePupilNumbers =
            UniquePupilNumbers.Create(
                uniquePupilNumbers: input.MyPupils?.Pupils?.Select(t => t.UPN).ToUniquePupilNumbers() ?? []);

        return new(uniquePupilNumbers);
    }
}
