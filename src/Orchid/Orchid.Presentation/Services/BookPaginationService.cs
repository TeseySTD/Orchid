using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orchid.Application.Common.Services;
using Orchid.Application.Services;
using Orchid.Core.Models.ValueObjects;
using Orchid.Presentation.Models;

namespace Orchid.Presentation.Services;

public class BookPaginationService : IDisposable
{
    private CancellationTokenSource? _calcPagesCts;
    private readonly IPaginationCacheService _paginationCacheService;

    public BookPaginationService(IPaginationCacheService paginationCacheService)
    {
        _paginationCacheService = paginationCacheService;
    }

    public void StartBackgroundPageCalculation(
        BookId bookId,
        IEnumerable<Chapter> chapters,
        ElementReference element,
        ChapterPaginationStore store,
        Func<int, int, Task> onChapterPagesCalculated,
        Func<Task> onAllPagesCalculated,
        IJSRuntime jsRuntime)
    {
        StopCalculation();
        _calcPagesCts = new CancellationTokenSource();

        _ = BackgroundPageCalculation(
            bookId,
            chapters,
            element,
            store,
            onChapterPagesCalculated,
            onAllPagesCalculated,
            jsRuntime,
            _calcPagesCts.Token
        );
    }

    private async Task BackgroundPageCalculation(
        BookId bookId,
        IEnumerable<Chapter> chapters,
        ElementReference element,
        ChapterPaginationStore store,
        Func<int, int, Task> onChapterCalculated,
        Func<Task> onAllCalculated,
        IJSRuntime jsRuntime,
        CancellationToken ct)
    {
        var context = await GetPaginationContext(element, jsRuntime);
        if (context == null) return;

        store.Init(bookId, context);
        var chapterList = chapters.ToList();

        try
        {
            for (int i = 0; i < chapterList.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                int pagesCount;

                if (_paginationCacheService.ChapterExists(bookId, context, i))
                {
                    var cachedPages = await _paginationCacheService.GetChapterAsync(bookId, context, i);
                    pagesCount = cachedPages?.Length ?? 0;
                }
                else
                {
                    var pages = await CalculateSingleChapterAsync(chapterList[i], element, jsRuntime, ct);
                    await _paginationCacheService.SaveChapterAsync(bookId, context, i, pages);
                    store.Invalidate(i);
                    pagesCount = pages.Length;
                }

                await onChapterCalculated(i, pagesCount);
                await Task.Delay(20, ct);
            }

            await onAllCalculated();
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            await jsRuntime.InvokeVoidAsync("orchidReader.cleanupSandbox");
        }
    }

    private async Task<PageData[]> CalculateSingleChapterAsync(
        Chapter chapter,
        ElementReference element,
        IJSRuntime jsRuntime,
        CancellationToken ct)
    {
        using var memStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(chapter.Html));
        using var streamRef = new DotNetStreamReference(memStream);

        await using var jsRef = await jsRuntime.InvokeAsync<IJSStreamReference>(
            "orchidReader.measureHiddenChapter", ct, element, streamRef);

        await using var pageStream = await jsRef.OpenReadStreamAsync(100_000_000, ct);

        return await JsonSerializer.DeserializeAsync<PageData[]>(
            pageStream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            ct) ?? [];
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

    public void StopCalculation()
    {
        _calcPagesCts?.Cancel();
        _calcPagesCts?.Dispose();
        _calcPagesCts = null;
    }

    public int FindPageIndexByLocator(PageData[] pages, string targetLocator)
    {
        if (pages.Length == 0) return 0;
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