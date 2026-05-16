using Orchid.Presentation.Services;

namespace Orchid.Presentation;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly MainPage _mainPage;

    public App(MainPage mainPage, LocalizationStateService localizationState)
    {
        InitializeComponent();
        localizationState.Initialize();
        _mainPage = mainPage;
    }


    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_mainPage) { Title = "Orchid" };
    }
}