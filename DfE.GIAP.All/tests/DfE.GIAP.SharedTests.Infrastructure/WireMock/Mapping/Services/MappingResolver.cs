using Newtonsoft.Json;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Services;
internal sealed class MappingResolver
{
    private readonly string _rootDirectory;

    public MappingResolver(string? rootDirectory)
    {
        _rootDirectory =
            !string.IsNullOrEmpty(rootDirectory) ?
                rootDirectory! :
                    $".{Path.PathSeparator}";

    }

    public MappingModel ResolveMapping(string path)
    {
        Guard.ThrowIfNullOrEmpty(path, nameof(path));

        string resolvedPath = ResolveMappingFilePath(path);

        return JsonConvert.DeserializeObject<MappingModel>(
            File.ReadAllText(resolvedPath)) ??
                throw new ArgumentException($"Unable to deserialise contents to MappingModel for resolved path {resolvedPath}");
    }

    // /contracts/
    // contracts/
    // TODO no way to distinguish if it should be abs or rel
    private string ResolveMappingFilePath(string filePath)
    {
        Guard.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

        if (filePath.StartsWith(_rootDirectory)) // /contracts or contracts
        {
            // Resolve from the current context
            return filePath.TrimStart(Path.PathSeparator, Path.DirectorySeparatorChar);
        }

        if (Path.IsPathRooted(filePath)) // e.g /absolutepath/to/contracts
        {
            return filePath;
        }

        return Path.Combine(_rootDirectory, filePath);
    }
}
