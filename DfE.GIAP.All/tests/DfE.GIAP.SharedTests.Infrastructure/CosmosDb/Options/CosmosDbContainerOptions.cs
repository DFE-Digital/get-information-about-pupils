namespace DfE.GIAP.SharedTests.Infrastructure.CosmosDb.Options;
public record CosmosDbContainerOptions
{
    public string ContainerName { get; }
    public string PartitionKey { get; }
    public PartitionKeyType PartitionKeyType { get; }

    public CosmosDbContainerOptions(string containerName, string partitionKey = "/id", PartitionKeyType type = PartitionKeyType.String)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
        ContainerName = containerName;

        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        PartitionKey = partitionKey.StartsWith('/') ? partitionKey : $"/{partitionKey}";

        PartitionKeyType = type;
    }
}

public enum PartitionKeyType
{
    Integer,
    String
}
