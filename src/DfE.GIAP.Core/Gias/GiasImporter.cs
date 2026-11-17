using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.Gias;

/// <remarks>
/// Both extracts are downloaded and validated in full before the database is touched; if either is unavailable
/// or looks wrong the existing data is left as it is.
/// </remarks>
public class GiasImporter(
    GiasExtractClient extractClient,
    GiasDataWriter dataWriter,
    TimeProvider timeProvider,
    ILogger<GiasImporter> logger)
{
    public async Task<GiasImportResult> ImportAsync(CancellationToken cancellationToken = default)
    {
        DateOnly date = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        GiasEstablishmentsExtract establishments;
        await using (Stream stream = await extractClient.GetExtractAsync(GiasExtractType.AllEstablishmentData, date, cancellationToken))
        {
            establishments = GiasExtractParser.ParseAllEstablishmentData(stream);
        }

        GiasGroupLinksExtract groupLinks;
        await using (Stream stream = await extractClient.GetExtractAsync(GiasExtractType.AllGroupLinks, date, cancellationToken))
        {
            groupLinks = GiasExtractParser.ParseAllGroupLinks(stream);
        }

        GiasImportResult result = await dataWriter.ReplaceAllAsync(establishments, groupLinks, cancellationToken);

        logger.LogInformation(
            "Imported GIAS extracts for {Date}: {LocalAuthorityCount} local authorities, {EstablishmentCount} establishments, {GroupCount} groups and {GroupLinkCount} group links.",
            date,
            result.LocalAuthorityCount,
            result.EstablishmentCount,
            result.GroupCount,
            result.GroupLinkCount);

        return result;
    }
}
