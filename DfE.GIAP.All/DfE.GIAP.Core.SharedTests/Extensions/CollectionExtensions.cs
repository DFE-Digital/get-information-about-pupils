namespace DfE.GIAP.Core.SharedTests.Extensions;
public static class CollectionExtensions
{
    public static string Merge(this IEnumerable<IEnumerable<char>> input, char separator = ' ') => string.Join(separator, input);
}
