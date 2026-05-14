namespace Orchid.Presentation;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly MainPage _mainPage;

    public App(MainPage mainPage)
    {
        InitializeComponent();
        _mainPage = mainPage;
    }


    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_mainPage) { Title = "Orchid" };
    }
}