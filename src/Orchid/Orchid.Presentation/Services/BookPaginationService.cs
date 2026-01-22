using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orchid.Application.Common.Services;
using Orchid.Application.Services;
using Orchid.Core.Models;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Presentation.Services;

public class BookPaginationService : IDisposable
{
    private CancellationTokenSource? _calcPagesCts;
    private readonly IPaginationCacheService _paginationCacheService;

    public BookPaginationService(IPaginationCacheService paginationCacheService)
    {
        _paginationCacheService = paginationCacheService;
    }

    private async Task<Dictionary<int, int>> CalculateAllChaptersPagesAsync(
        IEnumerable<Chapter> chapters,
        ElementReference element,
        Func<int, int, Task> onPagesCalculated,
        IJSRuntime jsRuntime,
        CancellationToken cancellationToken)
    {
        int i = 0;
        Dictionary<int, int> result = new();

        foreach (var chapter in chapters)
        {
            var memStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(chapter.Html));
            Stream fileStream = memStream;
            var streamRef = new DotNetStreamReference(memStream);

            try
            {
                var pages = await jsRuntime.InvokeAsync<int>(
                    "orchidReader.measureHiddenChapter",
                    cancellationToken,
                    element,
                    streamRef
                );

                await onPagesCalculated(i, pages);
                result.Add(i, pages);
            }
            finally
            {
                streamRef.Dispose();
                await fileStream.DisposeAsync();
            }

            i++;
            Console.WriteLine($"Pages calculated for chapter {i}");
            await Task.Delay(20, cancellationToken); // Wait for not to overload the render
        }

        return result;
    }


    private async Task BackgroundPageCalculation(
        BookId bookId,
        IEnumerable<Chapter> chapters,
        ElementReference element,
        Func<int, int, Task> onChapterPagesCalculated,
        IJSRuntime jsRuntime,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Calculating chapters for {element}");
        var paginationContext = await GetPaginationContext(element, jsRuntime);
        if (paginationContext == null)
        {
            Console.WriteLine("No pagination context. Stops calculation.");
            StopCalculation();
            return;
        }
            
        var cachedValue = await _paginationCacheService.GetMapAsync(bookId, paginationContext);
        if (cachedValue is not null)
        {
            Console.WriteLine("Getting pagination data from cache...");

            foreach (var (chapterIndex, pages) in cachedValue)
            {
                await onChapterPagesCalculated(chapterIndex, pages);
            }

            Console.WriteLine("All pages calculated");
            return;
        }

        try
        {
            var pages = await CalculateAllChaptersPagesAsync(
                chapters,
                element,
                onChapterPagesCalculated,
                jsRuntime,
                cancellationToken
            );
            await _paginationCacheService.SaveMapAsync(bookId, paginationContext, pages);
            Console.WriteLine("All pages calculated");
        }
        catch (OperationCanceledException)
        {
            // Ignore 
        }
        finally
        {
            await jsRuntime.InvokeVoidAsync("orchidReader.cleanupSandbox");
        }
    }

    public void StartBackgroundPageCalculation(
        BookId bookId,
        IEnumerable<Chapter> chapters,
        ElementReference element,
        Func<int, int, Task> onChapterPagesCalculated,
        IJSRuntime jsRuntime)
    {
        _calcPagesCts?.Cancel();
        _calcPagesCts?.Dispose();
        _calcPagesCts = new CancellationTokenSource();

        _ = BackgroundPageCalculation(
            bookId,
            chapters,
            element,
            onChapterPagesCalculated,
            jsRuntime,
            _calcPagesCts.Token
        );
    }

    public void StopCalculation()
    {
        if (_calcPagesCts != null)
        {
            _calcPagesCts.Cancel();
            _calcPagesCts.Dispose();
            _calcPagesCts = null;
        }
    }

    public async Task<string> GetCurrentPageLocator(ElementReference chapterElement, IJSRuntime jsRuntime)
    {
        try
        {
            return await jsRuntime.InvokeAsync<string>("orchidReader.getCurrentLocator", chapterElement);
        }
        catch (JSException)
        {
            return string.Empty;
        }
    }

    public async Task<PaginationContext?> GetPaginationContext(ElementReference chapterElement, IJSRuntime jsRuntime)
    {
        try
        {
            return await jsRuntime.InvokeAsync<PaginationContext>("utils.getPaginationContext", chapterElement);
        }
        catch (JSException)
        {
            return null;
        }
    }

    public void Dispose()
    {
        StopCalculation();
    }
}