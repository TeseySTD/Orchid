using Microsoft.VisualBasic.FileIO;
using Orchid.Application.Common.Repo;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Infrastructure.Data.Repo;

public class ImagesRepository : IImagesRepository
{
    public async Task SaveImagesAsync(IEnumerable<Image> images, string cacheDirectory)
    {
        FileSystem.CreateDirectory(cacheDirectory);

        foreach (var image in images)
        {
            var filePath = Path.Combine(cacheDirectory, image.Name);
            var directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory))
                FileSystem.CreateDirectory(directory);

            await File.WriteAllBytesAsync(filePath, image.Data);
        }
    }

    public bool ImageExists(string imageName, string cacheDirectory)
    {
        return File.Exists(Path.Combine(cacheDirectory, imageName));
    }
}