using ADIN.Avalonia.Commands;
using ADIN.Avalonia.Services;
using ADIN.Avalonia.Stores;
using ADIN.Avalonia.Views;
using ADIN.Device.Services;
using ADIN.Register.Services;
using FTDIChip.Driver.Services;
using System.Windows.Input;

namespace ADIN.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly SelectedDeviceStore _selectedDeviceStore;
    private readonly IFTDIServices _ftdiService;
    private readonly NavigationStore _navigationStore;
    private object _currentStatusView;

    public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;

    public MainWindowViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, NavigationStore navigationStore, IRegisterService registerService, ScriptService scriptService, ApplicationConfigService appConfigService, object mainLock)
    {
        _selectedDeviceStore = selectedDeviceStore;
        _ftdiService = ftdiService;
        _navigationStore = navigationStore;

        LogActivityVM = new LogActivityViewModel(_selectedDeviceStore);
        DeviceListingVM = new DeviceListingViewModel(_selectedDeviceStore, ftdiService, registerService, LogActivityVM, appConfigService, mainLock);
        DeviceStatusVM = new DeviceStatusViewModel(_selectedDeviceStore, _ftdiService, mainLock);
        ExtraCommandsVM = new ExtraCommandsViewModel(_selectedDeviceStore, _ftdiService, _navigationStore);

        NavigateLinkPropertiesCommand = new NavigateCommand<LinkPropertiesViewModel>(new NavigationService<LinkPropertiesViewModel>(_navigationStore, () => new LinkPropertiesViewModel(_selectedDeviceStore, _ftdiService)));
        NavigateLoopbackFrameGenCommand = new NavigateCommand<LoopbackFrameGenViewModel>(new NavigationService<LoopbackFrameGenViewModel>(_navigationStore, () => new LoopbackFrameGenViewModel(_selectedDeviceStore, _ftdiService)));
        NavigateRegisterAccessCommand = new NavigateCommand<RegisterListingViewModel>(new NavigationService<RegisterListingViewModel>(_navigationStore, () => new RegisterListingViewModel(_navigationStore)));

        _navigationStore.CurrentStatusView = new DeviceStatusView { DataContext = DeviceStatusVM };
        _navigationStore.CurrentViewModel = new LinkPropertiesViewModel(_selectedDeviceStore, _ftdiService);

        _navigationStore.CurrentViewModelChanged += _navigationStore_CurrentViewModelChanged;
    }

    public object CurrentStatusView
    {
        get
        {
            if (_navigationStore.CurrentViewModel is LinkPropertiesViewModel)
            {
                _navigationStore.CurrentStatusView = new DeviceStatusView { DataContext = DeviceStatusVM };
                return _navigationStore.CurrentStatusView;
            }
            else if (_navigationStore.CurrentViewModel is LoopbackFrameGenViewModel)
            {
                _navigationStore.CurrentStatusView = new FrameStatusView { DataContext = DeviceStatusVM };
                return _navigationStore.CurrentStatusView;
            }
            else
            {
                _navigationStore.CurrentStatusView = null;
                return _navigationStore.CurrentStatusView;
            }
        }
    }

    public int ColumnSpan => (_navigationStore.CurrentViewModel is RegisterListingViewModel) ? 2 : 1;

    public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;

    public DeviceListingViewModel DeviceListingVM { get; }
    public LogActivityViewModel LogActivityVM { get; set; }
    public DeviceStatusViewModel DeviceStatusVM { get; set; }
    public ExtraCommandsViewModel ExtraCommandsVM { get; set; }

    public ICommand NavigateLinkPropertiesCommand { get; }
    public ICommand NavigateLoopbackFrameGenCommand { get; }
    public ICommand NavigateRegisterAccessCommand { get; }

    private void _navigationStore_CurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
        OnPropertyChanged(nameof(CurrentStatusView));
        OnPropertyChanged(nameof(ColumnSpan));
    }
}
