namespace DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;
public interface IBlobStoragePathBuilder
{
    string BuildPath(BlobStoragePathContext context);
}

public class BlobStoragePathResolver : IBlobStoragePathBuilder
{
    private readonly IEnumerable<IBlobStoragePathStrategy> _strategies;

    public BlobStoragePathResolver(IEnumerable<IBlobStoragePathStrategy> strategies)
    {
        _strategies = strategies;
    }

    public string BuildPath(BlobStoragePathContext context)
    {
        IBlobStoragePathStrategy? strategy = _strategies.FirstOrDefault(s => s.CanHandle(context.OrganisationType));
        return strategy?.ResolvePath(context) ?? string.Empty;
    }
}
