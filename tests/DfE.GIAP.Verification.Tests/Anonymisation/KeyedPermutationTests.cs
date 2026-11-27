using System.Numerics;
using DfE.GIAP.Verification.Anonymisation;

namespace DfE.GIAP.Verification.Tests.Anonymisation;

public class KeyedPermutationTests
{
    private static readonly byte[] _key = [.. Enumerable.Range(0, 32).Select(i => (byte)i)];

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(1000)]
    [InlineData(1001)]
    public void Permute_EveryValue_IsAPermutationOfTheDomain(int size)
    {
        using KeyedPermutation permutation = new(_key, size);

        BigInteger[] permuted = [.. Enumerable.Range(0, size).Select(i => permutation.Permute(i))];

        Assert.Equal(Enumerable.Range(0, size).Select(i => (BigInteger)i), permuted.Order());
    }

    [Fact]
    public void Permute_WithADomainPredicate_IsAPermutationOfTheValuesItAccepts()
    {
        using KeyedPermutation permutation = new(_key, 1000);
        static bool IsEven(BigInteger value) => value.IsEven;

        BigInteger[] evens = [.. Enumerable.Range(0, 500).Select(i => (BigInteger)(i * 2))];
        BigInteger[] permuted = [.. evens.Select(v => permutation.Permute(v, IsEven))];

        Assert.Equal(evens, permuted.Order());
    }

    [Fact]
    public void Permute_IsTheSameForTheSameKey_AndDifferentForAnother()
    {
        using KeyedPermutation permutation = new(_key, BigInteger.Pow(10, 12));
        using KeyedPermutation same = new([.. _key], BigInteger.Pow(10, 12));
        using KeyedPermutation other = new([.. _key.Reverse()], BigInteger.Pow(10, 12));

        BigInteger[] values = [.. Enumerable.Range(0, 100).Select(i => (BigInteger)i * 7_919_000_001)];

        Assert.Equal(values.Select(v => permutation.Permute(v)), values.Select(v => same.Permute(v)));
        Assert.NotEqual(values.Select(v => permutation.Permute(v)), values.Select(v => other.Permute(v)));
    }

    [Fact]
    public void Permute_AValueOutsideTheDomain_Throws()
    {
        using KeyedPermutation permutation = new(_key, 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => permutation.Permute(10));
        Assert.Throws<ArgumentOutOfRangeException>(() => permutation.Permute(3, v => v != 3));
    }
}
