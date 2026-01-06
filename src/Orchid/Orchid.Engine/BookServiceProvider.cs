using Orchid.Engine.Common;
using Orchid.Engine.Epub;

namespace Orchid.Engine;

public class BookServiceProvider : IBookServiceProvider
{
    public IBookService GetService(string bookPath)
    {
        var extension = Path.GetExtension(bookPath);

        switch (extension)
        {
            case ".epub":
                return new EpubBookService();
            default:
                throw new ArgumentException($"Invalid book extension: {extension}");
        }
    }

    public IBookService GetService<TBookService>() where TBookService : IBookService
    {
        if (typeof(TBookService) == typeof(EpubBookService))
        {
            return new EpubBookService();
        }

        throw new ArgumentException($"Invalid type of book service: {typeof(TBookService)}");
    }

}