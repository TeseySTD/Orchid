using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Common.Repo;

public interface IImagesRepository
{
    public Task SaveImagesAsync(IEnumerable<Image> images, string cacheDirectory);
    public bool ImageExists(string imageName, string cacheDirectory);
}