using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Common.Repo;

public interface IImagesRepository
{
    public Task SaveImageAsync(Image image);
    public bool ImageExists(string imageName);
}