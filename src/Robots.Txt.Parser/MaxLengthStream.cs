using System;
using System.IO;

namespace Robots.Txt.Parser;

public class MaxLengthStream(Stream inner, long maxBytes) : Stream
{
    private readonly Stream _inner = inner;
    private readonly long _maxBytes = maxBytes;
    private long _bytesRead;

    public override int Read(byte[] buffer, int offset, int count)
    {
        int read = _inner.Read(buffer, offset, count);
        _bytesRead += read;

        if (_bytesRead > _maxBytes) throw new InvalidOperationException($"Decompressed content exceeded the limit of {_maxBytes} bytes");

        return read;
    }

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => _bytesRead;
        set => throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _inner.Dispose();
        base.Dispose(disposing);
    }

    public override void Flush() => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
