using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orchid.Application.Common.Services;
using Orchid.Application.Services;
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

    private async Task<Dictionary<int, string[]>> CalculateAllChaptersPagesAsync(
        IEnumerable<Chapter> chapters,
        ElementReference element,
        Func<int, string[], Task> onPagesCalculated,
        Func<Task> onAllPagesCalculated,
        IJSRuntime jsRuntime,
        CancellationToken cancellationToken)
    {
        int i = 0;
        Dictionary<int, string[]> result = new();

        foreach (var chapter in chapters)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var memStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(chapter.Html));
            var streamRef = new DotNetStreamReference(memStream);

            try
            {
                await using var jsStreamReference = await jsRuntime.InvokeAsync<IJSStreamReference>(
                    "orchidReader.measureHiddenChapter",
                    cancellationToken,
                    element,
                    streamRef
                );

                // Assuming 100MB max size for the chapter HTML array
                await using var pageStream = await jsStreamReference.OpenReadStreamAsync(maxAllowedSize: 100_000_000, cancellationToken);
                var pages = await JsonSerializer.DeserializeAsync<string[]>(pageStream, cancellationToken: cancellationToken) 
                            ?? Array.Empty<string>();

                await onPagesCalculated(i, pages);
                result.Add(i, pages);
            }
            finally
            {
                streamRef.Dispose();
                await memStream.DisposeAsync();
            }

            i++;
            await Task.Delay(20, cancellationToken); // Yield to not block the UI thread
        }

        await onAllPagesCalculated();
        return result;
    }

    private async Task BackgroundPageCalculation(
        BookId bookId,
        IEnumerable<Chapter> chapters,
        ElementReference element,
        Func<int, string[], Task> onChapterPagesCalculated,
        Func<Task> onAllPagesCalculated,
        IJSRuntime jsRuntime,
        CancellationToken cancellationToken)
    {
        var paginationContext = await GetPaginationContext(element, jsRuntime);
        if (paginationContext == null)
        {
            StopCalculation();
            return;
        }
            
        // var cachedValue = await _paginationCacheService.GetMapAsync(bookId, paginationContext);
        // if (cachedValue is not null)
        // {
        //     foreach (var (chapterIndex, pages) in cachedValue)
        //     {
        //         cancellationToken.ThrowIfCancellationRequested();
        //         await onChapterPagesCalculated(chapterIndex, pages);
        //     }
        //     return;
        // }

        try
        {
            var pages = await CalculateAllChaptersPagesAsync(
                chapters,
                element,
                onChapterPagesCalculated,
                onAllPagesCalculated,
                jsRuntime,
                cancellationToken
            );
            // await _paginationCacheService.SaveMapAsync(bookId, paginationContext, pages);
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
        Func<int, string[], Task> onChapterPagesCalculated,
        Func<Task> onAllPagesCalculated,
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
            onAllPagesCalculated,
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
