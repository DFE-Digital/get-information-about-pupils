namespace DfE.GIAP.SharedTests.Infrastructure.CosmosDb.Options;
public record CosmosDbContainerOptions
{
    public string ContainerName { get; }
    public string PartitionKey { get; }

    public CosmosDbContainerOptions(string containerName, string partitionKey = "/id")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
        ContainerName = containerName;

        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        PartitionKey = partitionKey.StartsWith('/') ? partitionKey : $"/{partitionKey}";
    }
}
