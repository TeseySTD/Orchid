using System;
using Orchid.Core.Models;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Engine.Common;

public interface IBookService
{
    Task<Book> ReadAsync(string bookFilePath);
    
    Task<Chapter> ReadChapterAsync(string bookFilePath, int chapterIndex);
}
