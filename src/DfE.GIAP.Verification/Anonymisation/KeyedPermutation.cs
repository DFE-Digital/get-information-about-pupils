using System.Numerics;
using System.Security.Cryptography;

namespace DfE.GIAP.Verification.Anonymisation;

/// <summary>
/// A keyed permutation of the integers <c>[0, domainSize)</c>: every value maps to exactly one other, so two real
/// values can never get the same fake one, and the same key always gives the same mapping.
/// </summary>
/// <remarks>
/// A balanced Feistel network over <c>[0, m²)</c>, where <c>m²</c> is the smallest square at least the size of the
/// domain, with HMAC-SHA256 as the round function. A value that lands outside the domain is permuted again until it's
/// back in it (cycle-walking), which keeps it a permutation of the domain.
/// </remarks>
public sealed class KeyedPermutation : IDisposable
{
    private const int Rounds = 10;

    private readonly HMACSHA256 _hmac;
    private readonly BigInteger _domainSize;
    private readonly BigInteger _half;

    public KeyedPermutation(byte[] key, BigInteger domainSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(domainSize, BigInteger.One);

        _hmac = new HMACSHA256(key);
        _domainSize = domainSize;
        _half = CeilingSquareRoot(domainSize);
    }

    public BigInteger DomainSize => _domainSize;

    public BigInteger Permute(BigInteger value) => Permute(value, static _ => true);

    /// <summary>
    /// Permutes the values for which <paramref name="isInDomain"/> is true among themselves; a value for which it's
    /// false is permuted again. <paramref name="value"/> must be one of them.
    /// </summary>
    public BigInteger Permute(BigInteger value, Func<BigInteger, bool> isInDomain)
    {
        if (value < 0 || value >= _domainSize || !isInDomain(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), "The value isn't in the domain.");
        }

        do
        {
            value = Feistel(value);
        }
        while (value >= _domainSize || !isInDomain(value));

        return value;
    }

    public void Dispose() => _hmac.Dispose();

    private BigInteger Feistel(BigInteger value)
    {
        BigInteger left = value / _half;
        BigInteger right = value % _half;

        for (byte round = 0; round < Rounds; round++)
        {
            (left, right) = (right, (left + RoundFunction(round, right)) % _half);
        }

        return (left * _half) + right;
    }

    private BigInteger RoundFunction(byte round, BigInteger value)
    {
        byte[] input = [round, .. value.ToByteArray(isUnsigned: true)];

        return new BigInteger(_hmac.ComputeHash(input), isUnsigned: true) % _half;
    }

    private static BigInteger CeilingSquareRoot(BigInteger value)
    {
        BigInteger root = (BigInteger)Math.Ceiling(Math.Sqrt((double)value));

        // Correct the double's rounding
        while (root * root < value)
        {
            root++;
        }

        while (root > 1 && (root - 1) * (root - 1) >= value)
        {
            root--;
        }

        return root;
    }
}
