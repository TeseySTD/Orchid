using Orchid.Application.Dto;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Common.Repo;

public interface IImagesRepository
{
    public Task SaveImageAsync(BookId id, Image image);
    public bool ImageExists(BookId id, string imageName);
    public string GetRelativeImagePath(BookId id, string imageName);
    Task<CacheSizeInfo> GetCacheSizeAsync(IEnumerable<string> excludedFileNames);
    void ClearCache(IEnumerable<string> excludedFileNames);
}