using ADIN.Avalonia.Commands;
using ADIN.Avalonia.Services;
using ADIN.Avalonia.Stores;
using FTDIChip.Driver.Services;
using System.Windows.Input;

namespace ADIN.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SelectedDeviceStore _selectedDeviceStore;
    private readonly IFTDIServices _ftdiService;
    private readonly NavigationStore _navigationStore;

    public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;

    public MainWindowViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, NavigationStore navigationStore)
    {
        _selectedDeviceStore = selectedDeviceStore;
        _ftdiService = ftdiService;
        _navigationStore = navigationStore;

        DeviceListingVM = new DeviceListingViewModel();
        NavigateLinkPropertiesCommand = new NavigateCommand<LinkPropertiesViewModel>(new NavigationService<LinkPropertiesViewModel>(_navigationStore, () => new LinkPropertiesViewModel(_navigationStore, _selectedDeviceStore)));
        NavigateLoopbackFrameGenCommand = new NavigateCommand<LoopbackFrameGenViewModel>(new NavigationService<LoopbackFrameGenViewModel>(_navigationStore, () => new LoopbackFrameGenViewModel(_navigationStore)));
        NavigateRegisterAccessCommand = new NavigateCommand<RegisterAccessViewModel>(new NavigationService<RegisterAccessViewModel>(_navigationStore, () => new RegisterAccessViewModel(_navigationStore)));

        DeviceListingViewModel = new DeviceListingViewModel(_selectedDeviceStore, _ftdiService);

        _navigationStore.CurrentViewModel = new LinkPropertiesViewModel(_navigationStore, _selectedDeviceStore);

        _navigationStore.CurrentViewModelChanged += _navigationStore_CurrentViewModelChanged;

    }
    public DeviceListingViewModel DeviceListingVM { get; }
    public ICommand NavigateLinkPropertiesCommand { get; }
    public ICommand NavigateLoopbackFrameGenCommand { get; }
    public ICommand NavigateRegisterAccessCommand { get; }

    private void _navigationStore_CurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
    }

    public DeviceListingViewModel DeviceListingViewModel { get; }
}
