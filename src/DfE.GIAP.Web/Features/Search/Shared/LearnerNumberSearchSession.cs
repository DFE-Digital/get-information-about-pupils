#nullable enable

using DfE.GIAP.Web.Shared.Serializer;

namespace DfE.GIAP.Web.Features.Search.Shared;

public static class LearnerNumberSearchSession
{
    public static List<string> GetMissingLearnerNumbers(ISession session, IJsonSerializer jsonSerializer, string sessionKey)
    {
        string? json = session.GetString(sessionKey);
        List<string>? missingLearnerNumbers = null;
        if (json is not null)
        {
            jsonSerializer.TryDeserialize(json, out missingLearnerNumbers);
        }

        return missingLearnerNumbers ?? [];
    }
}
