using Orchid.Application.Common.Providers;
using Orchid.Application.Common.Repo;
using Orchid.Application.Common.Services;
using Orchid.Application.Dto;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Infrastructure.Data.Repo;

public class ImagesRepository(IDiskCacheProvider diskCacheProvider) : IImagesRepository
{
    private const string ImagesCacheKey = "images";
    private IDiskCacheProvider DiskCacheProvider { get; } = diskCacheProvider;

    public async Task SaveImageAsync(BookId id, Image image)
    {
        await DiskCacheProvider.SaveBytesAsync(GetRelativeImagePath(id, image.Name), image.Data);
    }

    public bool ImageExists(BookId id, string imageName) =>
        DiskCacheProvider.Exists(GetRelativeImagePath(id, imageName));

    public string GetRelativeImagePath(BookId id, string imageName) =>
        Path.Combine(ImagesCacheKey, id.Value, imageName);

    public async Task<CacheSizeInfo> GetCacheSizeAsync(IEnumerable<string> excludedFileNames)
    {
        var sizes = await DiskCacheProvider.GetFolderSizeAsync(ImagesCacheKey, excludedFileNames);
        return new CacheSizeInfo(sizes.RemovableBytes, sizes.ExcludedBytes);
    }

    public void ClearCache(IEnumerable<string> excludedFileNames)
    {
        DiskCacheProvider.ClearFolder(ImagesCacheKey, excludedFileNames);
    }
}