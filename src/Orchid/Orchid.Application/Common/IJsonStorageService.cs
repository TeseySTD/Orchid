namespace Orchid.Application.Common;

public interface IJsonStorageService
{
    Task SaveAsync<T>(string key, T value);

    Task<T?> LoadAsync<T>(string key);

    Task<IEnumerable<T>> LoadAllInFolderAsync<T>(string folderName);

    void Delete(string key);
    
    bool Exists(string key);
}