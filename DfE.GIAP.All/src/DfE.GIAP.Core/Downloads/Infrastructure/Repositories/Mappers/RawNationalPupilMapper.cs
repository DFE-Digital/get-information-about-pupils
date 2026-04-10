using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers.Resolvers;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers;

public class RawNationalPupilMapper : IMapper<RawCosmosDocument, NationalPupil>
{
    public NationalPupil Map(RawCosmosDocument doc)
    {
        return GenericJsonToEntryResolver.Resolve<RawCosmosDocument, NationalPupil>(
            doc,
            NationalPupilInputSchema.Fields);
    }
}
