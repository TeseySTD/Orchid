using Orchid.Application.Common.Engine;
using Orchid.Engine.Epub;

namespace Orchid.Engine;

public class BookParserFactory : IBookParserFactory
{
    public IBookParser GetService(string bookPath)
    {
        var extension = Path.GetExtension(bookPath);

        switch (extension)
        {
            case ".epub":
                return new EpubBookParser();
            default:
                throw new ArgumentException($"Invalid book extension: {extension}");
        }
    }

    public IBookParser GetService<TBookService>() where TBookService : IBookParser
    {
        if (typeof(TBookService) == typeof(EpubBookParser))
        {
            return new EpubBookParser();
        }

        throw new ArgumentException($"Invalid type of book service: {typeof(TBookService)}");
    }

}