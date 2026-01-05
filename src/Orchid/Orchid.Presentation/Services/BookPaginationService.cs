using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Presentation.Services;

public class BookPaginationService : IDisposable
{
    private CancellationTokenSource? _calcPagesCts;

    private async Task CalculateAllChaptersPagesAsync(
        IEnumerable<Chapter> chapters,
        string allCss,
        ElementReference element,
        Func<int, int, Task> onPagesCalculated,
        IJSRuntime jsRuntime,
        CancellationToken cancellationToken)
    {
        int i = 0;

        foreach (var chapter in chapters)
        {
            if (string.IsNullOrEmpty(chapter.Html))
            {
                await onPagesCalculated(i, 0);
                i++;
                Console.WriteLine($"Pages calculated for chapter {i}");
                continue;
            }

            var pages = await jsRuntime.InvokeAsync<int>(
                "orchidReader.measureHiddenChapter",
                cancellationToken,
                element,
                chapter.Html,
                allCss
            );

            await onPagesCalculated(i, pages);
            i++;
            Console.WriteLine($"Pages calculated for chapter {i}");
            await Task.Delay(20, cancellationToken); // Wait for not to overload the render
        }
    }

    private async Task BackgroundPageCalculation(
        IEnumerable<Chapter> chapters,
        string allCss,
        ElementReference element,
        Func<int, int, Task> onChapterPagesCalculated,
        IJSRuntime jsRuntime,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Calculating chapters for {element}");
        try
        {
            await CalculateAllChaptersPagesAsync(
                chapters,
                allCss,
                element,
                onChapterPagesCalculated,
                jsRuntime,
                cancellationToken
            );
            Console.WriteLine($"All pages calculated");
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
        IEnumerable<Chapter> chapters,
        IEnumerable<CssFile> allCss,
        ElementReference element,
        Func<int, int, Task> onChapterPagesCalculated,
        IJSRuntime jsRuntime)
    {
        _calcPagesCts?.Cancel();
        _calcPagesCts?.Dispose();
        _calcPagesCts = new CancellationTokenSource();
        _ = BackgroundPageCalculation(
            chapters,
            string.Join("\n", allCss.Select(d => d.Content)),
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

    public void Dispose()
    {
        StopCalculation();
    }
}