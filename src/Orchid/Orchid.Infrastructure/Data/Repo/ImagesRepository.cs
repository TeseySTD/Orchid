using Orchid.Application.Common;
using Orchid.Application.Common.Repo;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Infrastructure.Data.Repo;

public class ImagesRepository(DiskCacheService diskCacheService) : IImagesRepository
{
    private DiskCacheService DiskCacheService { get; } = diskCacheService;
    public async Task SaveImagesAsync(IEnumerable<Image> images)
    {
        foreach (var image in images)
        {
            await DiskCacheService.SaveBytesAsync(image.Name, image.Data);
        }
    }

    public bool ImageExists(string imageName) => DiskCacheService.Exists(imageName);
}