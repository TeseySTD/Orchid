using Orchid.Application.Models;

namespace Orchid.Application.Common.Services;

public interface IAppSettingsService
{
    bool OpenLastBookOnStartup { get; set; }
    int ReaderFontSize { get; set; }
    double ReaderLineHeight { get; set; }
    AppThemeMode Theme { get; set; }
    string LastOpenedBookId { get; set; }
    
    event Action? OnSettingsChanged;
}