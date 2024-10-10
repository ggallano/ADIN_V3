using ADIN.Avalonia.Commands;
using ADIN.Avalonia.Services;
using ADIN.Avalonia.Stores;
using ADIN.Device.Services;
using ADIN.Register.Services;
using FTDIChip.Driver.Services;
using System.Windows.Input;

namespace ADIN.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SelectedDeviceStore _selectedDeviceStore;
    private readonly IFTDIServices _ftdiService;
    private readonly NavigationStore _navigationStore;

    public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;

    public MainWindowViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, NavigationStore navigationStore, IRegisterService registerService, ScriptService scriptService, object mainLock)
    {
        _selectedDeviceStore = selectedDeviceStore;
        _ftdiService = ftdiService;
        _navigationStore = navigationStore;

        LogActivityVM = new LogActivityViewModel(_selectedDeviceStore);
        DeviceListingVM = new DeviceListingViewModel(_selectedDeviceStore, ftdiService, registerService, LogActivityVM, mainLock);
        NavigateLinkPropertiesCommand = new NavigateCommand<LinkPropertiesViewModel>(new NavigationService<LinkPropertiesViewModel>(_navigationStore, () => new LinkPropertiesViewModel(_navigationStore, _selectedDeviceStore)));
        NavigateLinkProperties2Command = new NavigateCommand<LinkPropertiesViewModel2>(new NavigationService<LinkPropertiesViewModel2>(_navigationStore, () => new LinkPropertiesViewModel2(_navigationStore, _selectedDeviceStore)));
        NavigateLinkProperties3Command = new NavigateCommand<LinkPropertiesViewModel3>(new NavigationService<LinkPropertiesViewModel3>(_navigationStore, () => new LinkPropertiesViewModel3(_navigationStore, _selectedDeviceStore)));
        NavigateLoopbackFrameGenCommand = new NavigateCommand<LoopbackFrameGenViewModel>(new NavigationService<LoopbackFrameGenViewModel>(_navigationStore, () => new LoopbackFrameGenViewModel(_navigationStore)));
        NavigateRegisterAccessCommand = new NavigateCommand<RegisterAccessViewModel>(new NavigationService<RegisterAccessViewModel>(_navigationStore, () => new RegisterAccessViewModel(_navigationStore)));

        _navigationStore.CurrentViewModel = new LinkPropertiesViewModel(_navigationStore, _selectedDeviceStore);

        _navigationStore.CurrentViewModelChanged += _navigationStore_CurrentViewModelChanged;

    }
    public DeviceListingViewModel DeviceListingVM { get; }
    public LogActivityViewModel LogActivityVM { get; set; }
    public ICommand NavigateLinkPropertiesCommand { get; }
    public ICommand NavigateLinkProperties2Command { get; }
    public ICommand NavigateLinkProperties3Command { get; }
    public ICommand NavigateLoopbackFrameGenCommand { get; }
    public ICommand NavigateRegisterAccessCommand { get; }

    private void _navigationStore_CurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
    }
}
