using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orchid.Application.Common.Providers;
using Orchid.Application.Dto;
using Orchid.Core.Models.ValueObjects;
using Orchid.Presentation.Models;

namespace Orchid.Presentation.Services;

public class BookPaginationService : IDisposable
{
    private CancellationTokenSource? _calcPagesCts;
    private readonly IPaginationCacheProvider _paginationCacheProvider;

    private BookId? _runningBookId;
    private string? _runningContextHash;

    public BookPaginationService(IPaginationCacheProvider paginationCacheProvider)
    {
        _paginationCacheProvider = paginationCacheProvider;
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
        _ = ProcessPageCalculationAsync(
            bookId, chapters, element, store,
            onChapterPagesCalculated, onAllPagesCalculated, jsRuntime
        );
    }

    private async Task ProcessPageCalculationAsync(
        BookId bookId,
        IEnumerable<Chapter> chapters,
        ElementReference element,
        ChapterPaginationStore store,
        Func<int, int, Task> onChapterCalculated,
        Func<Task> onAllCalculated,
        IJSRuntime jsRuntime)
    {
        var context = await GetPaginationContext(element, jsRuntime);
        if (context == null) return;

        var hash = GenerateHash(context);
        
        // Check if book and context have changed to prevent race condition
        if (_runningBookId == bookId && _runningContextHash == hash &&
            _calcPagesCts is { IsCancellationRequested: false })
        {
            Debug.WriteLine(
                $"[Pagination] Calculation for Book {bookId.Value} with hash {hash} is already running. Skip restart.");
            return;
        }

        StopCalculation();

        _calcPagesCts = new CancellationTokenSource();
        _runningBookId = bookId;
        _runningContextHash = hash;

        var ct = _calcPagesCts.Token;
        var totalSw = Stopwatch.StartNew();

        Debug.WriteLine($"[Pagination] Start calculation for Book: {bookId.Value}, Hash: {hash}");

        try
        {
            store.Init(bookId, context);
            var chapterList = chapters.ToList();
            var manifest = await _paginationCacheProvider.GetManifestAsync(bookId, context) ?? [];

            await ProcessChaptersPaginationAsync(
                bookId, context, chapterList, manifest, element, store, onChapterCalculated, jsRuntime, ct);

            await onAllCalculated();

            totalSw.Stop();
            Debug.WriteLine(
                $"[Pagination] SUCCESS. All chapters calculated. Total time: {totalSw.ElapsedMilliseconds}ms");
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine($"[Pagination] Calculation cancelled for Book: {bookId.Value}");
            if (ct == _calcPagesCts?.Token) ResetRunningState();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Pagination] CRITICAL ERROR: {ex.Message}");
            ResetRunningState();
        }
        finally
        {
            if (!ct.IsCancellationRequested && ct == _calcPagesCts?.Token)
            {
                ResetRunningState();
            }

            await jsRuntime.InvokeVoidAsync("orchidReader.cleanupSandbox");
        }
    }

    private async Task ProcessChaptersPaginationAsync(
        BookId bookId,
        PaginationContext context,
        List<Chapter> chapterList,
        Dictionary<string, int> manifest,
        ElementReference element,
        ChapterPaginationStore store,
        Func<int, int, Task> onChapterCalculated,
        IJSRuntime jsRuntime,
        CancellationToken ct)
    {
        for (int i = 0; i < chapterList.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            int pagesCount;
            var chapterKey = i.ToString();
            bool fromManifest = false;
            string source;

            var chapterSw = Stopwatch.StartNew();

            if (manifest.TryGetValue(chapterKey, out var cachedCount) &&
                _paginationCacheProvider.ChapterExists(bookId, context, i))
            {
                pagesCount = cachedCount;
                fromManifest = true;
                source = "MANIFEST";
            }
            else if (_paginationCacheProvider.ChapterExists(bookId, context, i))
            {
                var cachedPages = await _paginationCacheProvider.GetChapterAsync(bookId, context, i);
                pagesCount = cachedPages?.Length ?? 0;

                if (pagesCount > 0 && cachedPages != null)
                {
                    manifest[chapterKey] = pagesCount;
                    await _paginationCacheProvider.SaveManifestAsync(bookId, context, manifest);
                }

                source = "CHAPTER_CACHE (Manifest repaired)";
            }
            else
            {
                var pages = await CalculateSingleChapterAsync(chapterList[i], element, jsRuntime, ct);
                await _paginationCacheProvider.SaveChapterAsync(bookId, context, i, pages);
                store.Invalidate(i);
                pagesCount = pages.Length;

                manifest[chapterKey] = pagesCount;
                await _paginationCacheProvider.SaveManifestAsync(bookId, context, manifest);
                source = "JS_ENGINE_CALCULATION";
            }

            chapterSw.Stop();
            Debug.WriteLine(
                $"[Pagination] Chapter [{i + 1}/{chapterList.Count}] -> Pages: {pagesCount} | Source: {source} | Time: {chapterSw.ElapsedMilliseconds}ms");

            await onChapterCalculated(i, pagesCount);

            if (!fromManifest)
            {
                await Task.Delay(20, ct);
            }
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
        ResetRunningState();
    }

    private void ResetRunningState()
    {
        _runningBookId = null;
        _runningContextHash = null;
    }

    private string GenerateHash(PaginationContext ctx)
    {
        var raw = $"{ctx.Width:F1}_{ctx.Height:F1}_{ctx.FontSize}_{ctx.FontFamily}_{ctx.LineHeight}";
        var bytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes)[..8];
    }

    public int FindPageIndexByLocator(PageData[] pages, string targetLocator)
    {
        if (pages.Length == 0 || string.IsNullOrEmpty(targetLocator)) return 0;
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