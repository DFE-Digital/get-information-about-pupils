using System.Text;
using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace DfE.GIAP.Verification.Anonymisation;

/// <summary>
/// Writes a block blob as a stream, staging its blocks as they fill and only committing them once
/// <see cref="CommitAsync"/> is called. Until then the blob doesn't exist, so an interrupted write leaves nothing
/// behind; uncommitted blocks are deleted by the storage account after a week.
/// </summary>
/// <remarks>
/// Only needs write permission, so it works with a SAS that can't read or list the container.
/// </remarks>
public sealed class BlockBlobWriteStream(BlockBlobClient blob) : Stream
{
    private const int BlockSize = 8 * 1024 * 1024;

    private readonly List<string> _blockIds = [];
    private readonly MemoryStream _block = new();
    private long _length;

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => _length;

    public override long Position
    {
        get => _length;
        set => throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        while (buffer.Length > 0)
        {
            int count = Math.Min(buffer.Length, BlockSize - (int)_block.Length);
            _block.Write(buffer[..count]);
            buffer = buffer[count..];
            _length += count;

            if (_block.Length == BlockSize)
            {
                StageBlock();
            }
        }
    }

    /// <summary>
    /// Commits the blob.
    /// </summary>
    /// <param name="overwrite">Whether to replace the blob if it already exists.</param>
    /// <returns><see langword="false"/> if the blob already exists and wasn't overwritten.</returns>
    public async Task<bool> CommitAsync(bool overwrite, CancellationToken cancellationToken = default)
    {
        if (_block.Length > 0 || _blockIds.Count == 0)
        {
            StageBlock();
        }

        try
        {
            await blob.CommitBlockListAsync(
                _blockIds,
                new CommitBlockListOptions { Conditions = overwrite ? null : new BlobRequestConditions { IfNoneMatch = ETag.All } },
                cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (!overwrite && ex.Status is 409 or 412)
        {
            return false;
        }
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _block.Dispose();
        }

        base.Dispose(disposing);
    }

    private void StageBlock()
    {
        string blockId = Convert.ToBase64String(Encoding.ASCII.GetBytes(_blockIds.Count.ToString("D6", System.Globalization.CultureInfo.InvariantCulture)));
        _block.Position = 0;
        blob.StageBlock(blockId, _block);
        _blockIds.Add(blockId);
        _block.SetLength(0);
    }
}
