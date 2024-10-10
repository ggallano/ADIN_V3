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

namespace ADIN.Avalonia;

public partial class App : Application
{
    private readonly SelectedDeviceStore _selectedDeviceStore;
    private IFTDIServices _ftdiService;
    private IRegisterService _registerService;
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
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(_selectedDeviceStore, _ftdiService, _navigationStore, _registerService, _scriptService, _mainLock),
            };
            desktop.MainWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }
}