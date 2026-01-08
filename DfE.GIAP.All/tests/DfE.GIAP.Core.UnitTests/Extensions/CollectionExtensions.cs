namespace DfE.GIAP.Core.UnitTests.Extensions;
internal static class CollectionExtensions
{
    internal static string Merge(this IEnumerable<IEnumerable<char>> input, char separator = ' ')
        => string.Join(separator, input);
}
