namespace Orchid.Application.Common.Engine;

public interface IBookParserFactory
{
    IBookParser GetService(string bookPath);
    IBookParser GetService<TBookService>() where TBookService : IBookParser;
}
