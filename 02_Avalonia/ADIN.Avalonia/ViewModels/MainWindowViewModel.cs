// <copyright file="MainWindowViewModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Avalonia.Commands;
using ADIN.Avalonia.Services;
using ADIN.Avalonia.Stores;
using ADIN.Avalonia.Views;
using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.Register.Models;
using ADIN.Register.Services;
using AnalogDevices.Desktop.Harmonic.Controls;
using AnalogDevices.Desktop.Harmonic.Extensions;
using AnalogDevices.Desktop.Harmonic.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using FTDIChip.Driver.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        RegisterAccessVM = new RegisterAccessViewModel(_selectedDeviceStore, _navigationStore);
        DeviceStatusVM = new DeviceStatusViewModel(_selectedDeviceStore, _ftdiService, mainLock);
        ExtraCommandsVM = new ExtraCommandsViewModel(_selectedDeviceStore, _ftdiService, _navigationStore);

        LinkPropertiesVM = new LinkPropertiesViewModel(_selectedDeviceStore, _ftdiService, appConfigService);
        LoopbackFrameGenVM = new LoopbackFrameGenViewModel(_selectedDeviceStore, _ftdiService, appConfigService);
        RegisterListingVM = new RegisterListingViewModel(_selectedDeviceStore, _ftdiService);

        NavigateLinkPropertiesCommand = new NavigateCommand<LinkPropertiesViewModel>(new NavigationService<LinkPropertiesViewModel>(_navigationStore, () => LinkPropertiesVM));
        NavigateLoopbackFrameGenCommand = new NavigateCommand<LoopbackFrameGenViewModel>(new NavigationService<LoopbackFrameGenViewModel>(_navigationStore, () => LoopbackFrameGenVM));
        NavigateRegisterAccessCommand = new NavigateCommand<RegisterListingViewModel>(new NavigationService<RegisterListingViewModel>(_navigationStore, () => RegisterListingVM));

        _navigationStore.CurrentViewModel = new RegisterListingViewModel(_selectedDeviceStore, _ftdiService);
        _navigationStore.CurrentViewModel = new LoopbackFrameGenViewModel(_selectedDeviceStore, _ftdiService, appConfigService);
        _navigationStore.CurrentViewModel = new LinkPropertiesViewModel(_selectedDeviceStore, _ftdiService, appConfigService);

        _navigationStore.CurrentViewModelChanged += UpdateViewViewModel;
        _selectedDeviceStore.PhyModeChanged += UpdateViewViewModel;
        _selectedDeviceStore.LoadingStatusChanged += UpdateLoadingStatus;
    }

    public ThemeVariant CurrentTheme => Application.Current.RequestedThemeVariant;

    public object CurrentView
    {
        get
        {
            if (_navigationStore.CurrentViewModel is LinkPropertiesViewModel && IsFiberMedia)
            {
                _navigationStore.CurrentView = new LinkPropertiesFiberView { DataContext = LinkPropertiesVM };
                return _navigationStore.CurrentView;
            }
            else if (_navigationStore.CurrentViewModel is LinkPropertiesViewModel && IsMediaConverter)
            {
                _navigationStore.CurrentView = new LinkPropertiesMedConvView { DataContext = LinkPropertiesVM };
                return _navigationStore.CurrentView;
            }
            else if (_navigationStore.CurrentViewModel is LinkPropertiesViewModel && IsCopperMedia)
            {
                _navigationStore.CurrentView = new LinkPropertiesView { DataContext = LinkPropertiesVM };
                return _navigationStore.CurrentView;
            }
            else if (_navigationStore.CurrentViewModel is LoopbackFrameGenViewModel)
            {
                _navigationStore.CurrentView = new LoopbackFrameGenView { DataContext = LoopbackFrameGenVM };
                return _navigationStore.CurrentView;
            }
            else if (_navigationStore.CurrentViewModel is RegisterListingViewModel)
            {
                _navigationStore.CurrentView = new RegisterListingView { DataContext = RegisterListingVM };
                return _navigationStore.CurrentView;
            }
            else
            {
                _navigationStore.CurrentView = null;
                return _navigationStore.CurrentView;
            }
        }
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
    public string _activePhyMode => _selectedDeviceStore.SelectedDevice?.PhyMode.ActivePhyMode;
    public bool IsCopperMedia => (_activePhyMode == null)
        || (_activePhyMode == "Copper Media Only")
        || (_activePhyMode == "Auto Media Detect_Cu");
    public bool IsFiberMedia => (_activePhyMode == "Fiber Media Only")
        || (_activePhyMode == "Backplane")
        || (_activePhyMode == "Auto Media Detect_Fi");
    public bool IsMediaConverter => _activePhyMode == "Media Converter";

    public DeviceListingViewModel DeviceListingVM { get; }
    public LogActivityViewModel LogActivityVM { get; set; }
    public RegisterAccessViewModel RegisterAccessVM { get; set; }
    public DeviceStatusViewModel DeviceStatusVM { get; set; }
    public ExtraCommandsViewModel ExtraCommandsVM { get; set; }

    public LinkPropertiesViewModel LinkPropertiesVM { get; set; }
    public LoopbackFrameGenViewModel LoopbackFrameGenVM { get; set; }
    public RegisterListingViewModel RegisterListingVM { get; set; }

    public ICommand NavigateLinkPropertiesCommand { get; }
    public ICommand NavigateLoopbackFrameGenCommand { get; }
    public ICommand NavigateRegisterAccessCommand { get; }

    private void UpdateViewViewModel()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
        OnPropertyChanged(nameof(CurrentView));
        OnPropertyChanged(nameof(CurrentStatusView));
        OnPropertyChanged(nameof(ColumnSpan));
        OnPropertyChanged(nameof(IsLoading));
    }

    public bool IsLoading { get; set; }
    public string LoadingString { get; set; }

    private void UpdateLoadingStatus(bool isLoading, string loadingString)
    {
        IsLoading = isLoading;
        LoadingString = loadingString;
        OnPropertyChanged(nameof(IsLoading));
        OnPropertyChanged(nameof(LoadingString));
    }
}
