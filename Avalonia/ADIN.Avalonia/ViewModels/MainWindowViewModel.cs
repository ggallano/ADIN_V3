using ADIN.Avalonia.Commands;
using ADIN.Avalonia.Services;
using ADIN.Avalonia.Stores;
using System.Windows.Input;

namespace ADIN.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
//#pragma warning disable CA1822 // Mark members as static
//    public string Greeting => "Welcome to Avalonia!";
//#pragma warning restore CA1822 // Mark members as static

    //private readonly SelectedDeviceStore _selectedDeviceStore;
    //private readonly IFTDIServices _ftdiService;
    private readonly NavigationStore _navigationStore;

    public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;

    //public OperationViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, NavigationStore navigationStore)
    public MainWindowViewModel(NavigationStore navigationStore)
    {
        //_selectedDeviceStore = selectedDeviceStore;
        //_ftdiService = ftdiService;
        _navigationStore = navigationStore;

        NavigateLinkPropertiesCommand = new NavigateCommand<LinkPropertiesViewModel>(new NavigationService<LinkPropertiesViewModel>(_navigationStore, () => new LinkPropertiesViewModel(_navigationStore)));
        NavigateLoopbackFrameGenCommand = new NavigateCommand<LoopbackFrameGenViewModel>(new NavigationService<LoopbackFrameGenViewModel>(_navigationStore, () => new LoopbackFrameGenViewModel(_navigationStore)));
        NavigateRegisterAccessCommand = new NavigateCommand<RegisterAccessViewModel>(new NavigationService<RegisterAccessViewModel>(_navigationStore, () => new RegisterAccessViewModel(_navigationStore)));

        //DeviceListingViewModel = new DeviceListingViewModel(_selectedDeviceStore, _ftdiService);

        _navigationStore.CurrentViewModel = new LinkPropertiesViewModel(_navigationStore);

        _navigationStore.CurrentViewModelChanged += _navigationStore_CurrentViewModelChanged;

    }
    public ICommand NavigateLinkPropertiesCommand { get; }
    public ICommand NavigateLoopbackFrameGenCommand { get; }
    public ICommand NavigateRegisterAccessCommand { get; }

    //private NavigationService<LinkPropertiesViewModel> CreateLinkPropNavigationService()
    //{
    //    return new NavigationService<LinkPropertiesViewModel>(_navigationStore, () => new LinkPropertiesViewModel(_navigationStore));
    //}

    //private NavigationService<RegisterAccessViewModel> CreateRegisterAccNavigationService()
    //{
    //    return new NavigationService<RegisterAccessViewModel>(_navigationStore, () => new RegisterAccessViewModel(_navigationStore));
    //}

    private void _navigationStore_CurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
    }

    //public DeviceListingViewModel DeviceListingViewModel { get; }
}
