namespace Orchid.Application.Models;

public record CacheSizeInfo(long RemovableBytes, long PersistentBytes)
{
    public long TotalBytes => RemovableBytes + PersistentBytes;
}