using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using ADIN.Avalonia.ViewModels;
using ADIN.Avalonia.Views;
using ADIN.Avalonia.Stores;
using ADIN.Register.Services;
using ADIN.Device.Services;
using FTDIChip.Driver.Services;
using ADIN.Avalonia.Services;
using Avalonia.Styling;

namespace ADIN.Avalonia;

public partial class App : Application
{
    private readonly SelectedDeviceStore _selectedDeviceStore;
    private IFTDIServices _ftdiService;
    private IRegisterService _registerService;
    private ApplicationConfigService _applicationConfigService;
    private ScriptService _scriptService;
    private object _mainLock = new object();
    private readonly NavigationStore _navigationStore;

    public App()
    {
        _selectedDeviceStore = new SelectedDeviceStore();
        _navigationStore = new NavigationStore();
        _ftdiService = new FTDIServices();
        _scriptService = new ScriptService();
        _registerService = new RegisterService();
        _applicationConfigService = new ApplicationConfigService();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            RequestedThemeVariant = ThemeVariant.Dark;
            desktop.MainWindow = new MainWindow()
            {
                DataContext = new MainWindowViewModel(_selectedDeviceStore, _ftdiService, _navigationStore, _registerService, _scriptService, _applicationConfigService, _mainLock),
            };
            desktop.MainWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }
}