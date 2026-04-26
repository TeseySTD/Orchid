using Orchid.Application.Common.Repo;
using Orchid.Application.Common.Services;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Infrastructure.Data.Repo;

public class ImagesRepository(DiskCacheService diskCacheService) : IImagesRepository
{
    private const string ImagesCacheKey = "images";
    private DiskCacheService DiskCacheService { get; } = diskCacheService;

    public async Task SaveImageAsync(BookId id, Image image)
    {
        await DiskCacheService.SaveBytesAsync(GetRelativeImagePath(id, image.Name), image.Data);
    }

    public bool ImageExists(BookId id, string imageName) => DiskCacheService.Exists(GetRelativeImagePath(id, imageName));

    public string GetRelativeImagePath(BookId id, string imageName) => Path.Combine(ImagesCacheKey, id.Value, imageName);
}