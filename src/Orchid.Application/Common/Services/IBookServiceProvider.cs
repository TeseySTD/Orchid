namespace Orchid.Application.Common.Services;

public interface IBookServiceProvider
{
    IBookService GetService(string bookPath);
    IBookService GetService<TBookService>() where TBookService : IBookService;
}
