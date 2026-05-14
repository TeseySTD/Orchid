using Orchid.Application.Common.Repo;
using Orchid.Application.Common.Services;
using Orchid.Application.Models;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Infrastructure.Data.Repo;

public class ImagesRepository(IDiskCacheService diskCacheService) : IImagesRepository
{
    private const string ImagesCacheKey = "images";
    private IDiskCacheService DiskCacheService { get; } = diskCacheService;

    public async Task SaveImageAsync(BookId id, Image image)
    {
        await DiskCacheService.SaveBytesAsync(GetRelativeImagePath(id, image.Name), image.Data);
    }

    public bool ImageExists(BookId id, string imageName) =>
        DiskCacheService.Exists(GetRelativeImagePath(id, imageName));

    public string GetRelativeImagePath(BookId id, string imageName) =>
        Path.Combine(ImagesCacheKey, id.Value, imageName);

    public async Task<CacheSizeInfo> GetCacheSizeAsync(IEnumerable<string> excludedFileNames)
    {
        var sizes = await DiskCacheService.GetFolderSizeAsync(ImagesCacheKey, excludedFileNames);
        return new CacheSizeInfo(sizes.RemovableBytes, sizes.ExcludedBytes);
    }

    public void ClearCache(IEnumerable<string> excludedFileNames)
    {
        DiskCacheService.ClearFolder(ImagesCacheKey, excludedFileNames);
    }
}