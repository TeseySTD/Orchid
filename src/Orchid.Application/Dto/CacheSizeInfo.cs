namespace Orchid.Application.Dto;

public record CacheSizeInfo(long RemovableBytes, long PersistentBytes)
{
    public long TotalBytes => RemovableBytes + PersistentBytes;
}