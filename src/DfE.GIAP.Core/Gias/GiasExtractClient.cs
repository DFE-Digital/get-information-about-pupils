using System.Globalization;

namespace DfE.GIAP.Core.Gias;

public class GiasExtractClient(HttpClient httpClient)
{
    /// <returns>A stream over the response body; the caller is responsible for disposing it.</returns>
    public virtual async Task<Stream> GetExtractAsync(GiasExtractType extractType, DateOnly date, CancellationToken cancellationToken = default)
    {
        string fileName = GetFileName(extractType, date);

        HttpResponseMessage response = await httpClient.GetAsync(fileName, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        try
        {
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    internal static string GetFileName(GiasExtractType extractType, DateOnly date)
    {
        string prefix = extractType switch
        {
            GiasExtractType.AllEstablishmentData => "edubasealldata",
            GiasExtractType.AllGroupLinks => "alllinksdata",
            _ => throw new ArgumentOutOfRangeException(nameof(extractType))
        };

        return $"{prefix}{date.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.csv";
    }
}
