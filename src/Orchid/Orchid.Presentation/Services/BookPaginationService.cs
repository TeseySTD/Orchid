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

    private async Task<Dictionary<int, PageData[]>> CalculateAllChaptersPagesAsync(
        IEnumerable<Chapter> chapters,
        ElementReference element,
        Func<int, PageData[], Task> onPagesCalculated,
        Func<Task> onAllPagesCalculated,
        IJSRuntime jsRuntime,
        CancellationToken cancellationToken)
    {
        int i = 0;
        Dictionary<int, PageData[]> result = new();

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
                await using var pageStream =
                    await jsStreamReference.OpenReadStreamAsync(maxAllowedSize: 100_000_000, cancellationToken);
                var pages = await JsonSerializer.DeserializeAsync<PageData[]>(
                                pageStream,
                                new JsonSerializerOptions(JsonSerializerDefaults.Web),
                                cancellationToken: cancellationToken)
                            ?? [];

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
        Func<int, PageData[], Task> onChapterPagesCalculated,
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

        var cachedValue = await _paginationCacheService.GetMapAsync(bookId, paginationContext);
        if (cachedValue is not null)
        {
            foreach (var (chapterIndex, pages) in cachedValue)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await onChapterPagesCalculated(chapterIndex, pages);
            }
            await onAllPagesCalculated();
            return;
        }

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
            await _paginationCacheService.SaveMapAsync(bookId, paginationContext, pages);
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
        Func<int, PageData[], Task> onChapterPagesCalculated,
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

    public int FindPageIndexByLocator(PageData[] pages, string targetLocator)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (CompareLocators(pages[i].Locator, targetLocator) > 0)
                return Math.Max(0, i - 1);
        }

        return pages.Length - 1;
    }

    private int CompareLocators(string loc1, string loc2)
    {
        var p1 = loc1.Split(':');
        var p2 = loc2.Split(':');
        var path1 = p1[0].Split('/').Select(int.Parse).ToArray();
        var path2 = p2[0].Split('/').Select(int.Parse).ToArray();

        for (int i = 0; i < Math.Min(path1.Length, path2.Length); i++)
        {
            if (path1[i] < path2[i]) return -1;
            if (path1[i] > path2[i]) return 1;
        }

        if (path1.Length != path2.Length) return path1.Length.CompareTo(path2.Length);

        return int.Parse(p1[1]).CompareTo(int.Parse(p2[1]));
    }

    public void Dispose()
    {
        StopCalculation();
    }
}