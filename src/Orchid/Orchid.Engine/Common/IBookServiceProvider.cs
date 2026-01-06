namespace Orchid.Engine.Common;

public interface IBookServiceProvider
{
    IBookService GetService(string bookPath);
    IBookService GetService<TBookService>() where TBookService : IBookService;
}
