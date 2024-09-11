// <copyright file="OperationViewModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADI.Register.Services;
using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace ADIN.WPF.ViewModel
{
    public class OperationViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly SelectedDeviceStore _selectedDeviceStore;

        private string _busyContent = string.Empty;
        private bool _enableTabs = false;
        private bool _isAdin2111 = false;
        private bool _isBusy = false;
        private List<string> _availableTabs = new List<string>()
        {
            "LinkProperties",
            "ClockPinControl",
            "Loopback",
            "FrameGenChecker",
            "TestModes",
            "CableDiagnostics",
            "RegisterAccess",
        };
        private string _selectedTab = string.Empty;
        private bool _isADIN1300CableDiagAvailable;
        private bool _isADIN1100CableDiagAvailable;

        private bool _isLinkPropVisible = true;
        private bool _isLinkPropSelected = true;
        private bool _isClkPinControlVisible = true;
        private bool _isClkPinControlSelected = false;
        private bool _isLoopbackVisible = true;
        private bool _isLoopbackSelected = false;
        private bool _isFrameGenVisible = true;
        private bool _isFrameGenSelected = false;
        private bool _isLoopbackFrameGenVisible = false;
        private bool _isLoopbackFrameGenSelected = false;
        private bool _isTestModeVisible = true;
        private bool _isTestModeSelected = false;
        private bool _isCableDiagVisible = true;
        private bool _isCableDiagSelected = false;

        public OperationViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, NavigationStore navigationStore, IRegisterService registerService, ScriptService scriptService, object mainLock)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _navigationStore = navigationStore;

            MenuItemVM = new MenuItemViewModel();
            LogActivityVM = new LogActivityViewModel(_selectedDeviceStore);
            DeviceListingVM = new DeviceListingViewModel(_selectedDeviceStore, ftdiService, registerService, LogActivityVM, mainLock);
            ExtraCommandsVM = new ExtraCommandsViewModel(_selectedDeviceStore, ftdiService);
            RegisterListingVM = new RegisterListingViewModel(selectedDeviceStore, ftdiService);
            LinkPropertiesVM = new LinkPropertiesViewModel(_navigationStore, _selectedDeviceStore);
            LoopbackVM = new LoopbackViewModel(_selectedDeviceStore, ftdiService);
            LoopbackFrameGenVM = new LoopbackFrameGenViewModel(_selectedDeviceStore, mainLock, ftdiService);
            FrameGenCheckerVM = new FrameGenCheckerViewModel(_selectedDeviceStore, mainLock, ftdiService);
            ClockPinControlVM = new ClockPinControlViewModel(_navigationStore, _selectedDeviceStore);
            TestModeVM = new TestModeViewModel(_selectedDeviceStore);
            DeviceStatusVM = new DeviceStatusViewModel(selectedDeviceStore, ftdiService, mainLock);
            RegisterAccessVM = new RegisterAccessViewModel(_selectedDeviceStore, navigationStore);
            RunCableDiagnosticVM = new RunCableDiagViewModel(_selectedDeviceStore, mainLock);
            TDRVM = new TimeDomainReflectometryViewModel(_selectedDeviceStore, mainLock);

            if (Properties.Settings.Default.ActiveLinkMon)
                ActiveLinkMonVM = new ActiveLinkMonViewModel(_selectedDeviceStore, mainLock);

            StatusStripVM = new StatusStripViewModel(_selectedDeviceStore, scriptService);

            _selectedTab = _availableTabs[0];

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.BusyStateChanged += _selectedDeviceStore_BusyStateChanged;
            _selectedDeviceStore.OnGoingCalibrationStatusChanged += _selectedDeviceStore_OnGoingCalibrationStatusChanged;
        }

        public ActiveLinkMonViewModel ActiveLinkMonVM { get; set; }

        public string BusyContent
        {
            get
            {
                return _busyContent;
            }

            set
            {
                _busyContent = value;
                OnPropertyChanged(nameof(BusyContent));
            }
        }

        public ClockPinControlViewModel ClockPinControlVM { get; set; }

        public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;

        public DeviceListingViewModel DeviceListingVM { get; }

        public DeviceStatusViewModel DeviceStatusVM { get; set; }

        public bool EnableTabs
        {
            get
            {
                return _enableTabs;
            }

            set
            {
                _enableTabs = value;
                OnPropertyChanged(nameof(EnableTabs));
            }
        }

        public ExtraCommandsViewModel ExtraCommandsVM { get; set; }

        public FrameGenCheckerViewModel FrameGenCheckerVM { get; set; }

        public bool IsActiveLinkMonEnabled { get; } = Properties.Settings.Default.ActiveLinkMon;

#if !DISABLE_TSN && !DISABLE_T1L
        public bool IsADIN1100Visible => !CheckGigabitBoard(_selectedDeviceStore.SelectedDevice?.DeviceType ?? BoardType.ADIN1300);

        public bool IsADIN1300Visible => CheckGigabitBoard(_selectedDeviceStore.SelectedDevice?.DeviceType ?? BoardType.ADIN1300);

#elif !DISABLE_TSN
        public bool IsADIN1300Visible { get; } = true;

        public bool IsADIN1100Visible { get; } = false;
#elif !DISABLE_T1L
        public bool IsADIN1300Visible { get; } = false;

        public bool IsADIN1100Visible { get; } = true;
#endif

        public bool IsADIN2111
        {
            get
            {
                return _isAdin2111;
            }

            set
            {
                _isAdin2111 = value;
                OnPropertyChanged(nameof(IsADIN2111));
            }
        }

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public bool IsLinkPropVisible
        {
            get
            {
                return _availableTabs.Contains("LinkProperties");
            }
        }

        public bool IsLinkPropSelected
        {
            get
            {
                return _selectedTab == "LinkProperties";
            }

            set
            {
                _selectedTab = "LinkProperties";
                UpdateSelectedTab();
            }
        }

//        public bool IsClkPinControlVisible
//        {
//            get
//            {
//#if DISABLE_T1L
//                return true;
//#else
//                if (_selectedDeviceStore.SelectedDevice == null)
//                    return false;

//                return _selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1300 || _selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1200;
//#endif
//            }
//        }

        public bool IsClkPinControlVisible
        {
            get
            {
                return _availableTabs.Contains("ClockPinControl");
            }
        }

        public bool IsClkPinControlSelected
        {
            get
            {
                return _selectedTab == "ClockPinControl";
            }

            set
            {
                _selectedTab = "ClockPinControl";
                UpdateSelectedTab();
            }
        }

        public bool IsLoopbackVisible
        {
            get
            {
                return _availableTabs.Contains("Loopback");
            }
        }

        public bool IsLoopbackSelected
        {
            get
            {
                return _selectedTab == "Loopback";
            }

            set
            {
                _selectedTab = "Loopback";
                UpdateSelectedTab();
            }
        }

        public bool IsFrameGenVisible
        {
            get
            {
                return _availableTabs.Contains("FrameGenChecker");
            }
        }

        public bool IsFrameGenSelected
        {
            get
            {
                return _selectedTab == "FrameGenChecker";
            }

            set
            {
                _selectedTab = "FrameGenChecker";
                UpdateSelectedTab();
            }
        }

        public bool IsLoopbackFrameGenVisible
        {
            get
            {
                return _availableTabs.Contains("LoopbackFrameGenChecker");
            }
        }

        public bool IsLoopbackFrameGenSelected
        {
            get
            {
                return _selectedTab == "LoopbackFrameGenChecker";
            }

            set
            {
                _selectedTab = "LoopbackFrameGenChecker";
                UpdateSelectedTab();
            }
        }

        public bool IsTestModeVisible
        {
            get
            {
                return _availableTabs.Contains("TestModes");
            }
        }

        public bool IsTestModeSelected
        {
            get
            {
                return _selectedTab == "TestModes";
            }

            set
            {
                _selectedTab = "TestModes";
                UpdateSelectedTab();
            }
        }

        public bool IsCableDiagVisible
        {
            get
            {
                return _availableTabs.Contains("CableDiagnostics") && (_isADIN1300CableDiagAvailable || _isADIN1100CableDiagAvailable);
            }
        }

        public bool IsCableDiagSelected
        {
            get
            {
                return _selectedTab == "CableDiagnostics";
            }

            set
            {
                _selectedTab = "CableDiagnostics";
                UpdateSelectedTab();
            }
        }

        public bool IsRegAccessSelected
        {
            get
            {
                return _selectedTab == "RegisterAccess";
            }

            set
            {
                _selectedTab = "RegisterAccess";
                UpdateSelectedTab();
            }
        }

        private void UpdateSelectedTab ()
        {
            OnPropertyChanged(nameof(IsLinkPropSelected));
            OnPropertyChanged(nameof(IsClkPinControlSelected));
            OnPropertyChanged(nameof(IsLoopbackSelected));
            OnPropertyChanged(nameof(IsFrameGenSelected));
            OnPropertyChanged(nameof(IsLoopbackFrameGenSelected));
            OnPropertyChanged(nameof(IsTestModeSelected));
            OnPropertyChanged(nameof(IsCableDiagSelected));
            OnPropertyChanged(nameof(IsRegAccessSelected));
        }

        public LinkPropertiesViewModel LinkPropertiesVM { get; set; }

        public LogActivityViewModel LogActivityVM { get; set; }

        public LoopbackViewModel LoopbackVM { get; set; }

        public LoopbackFrameGenViewModel LoopbackFrameGenVM { get; set; }

        public MenuItemViewModel MenuItemVM { get; set; }

        public RegisterAccessViewModel RegisterAccessVM { get; set; }

        public RegisterListingViewModel RegisterListingVM { get; set; }

        public StatusStripViewModel StatusStripVM { get; set; }

        public TimeDomainReflectometryViewModel TDRVM { get; set; }

        public TestModeViewModel TestModeVM { get; set; }

        public RunCableDiagViewModel RunCableDiagnosticVM { get; set; }

        private void _selectedDeviceStore_BusyStateChanged(string busyContent)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                BusyContent = busyContent;
                if (BusyContent == null || BusyContent == string.Empty || BusyContent == "Done")
                    IsBusy = false;
                else
                    IsBusy = true;
            }));
        }

        private void _selectedDeviceStore_OnGoingCalibrationStatusChanged(bool onGoingCalibrationStatus)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                EnableTabs = !onGoingCalibrationStatus;
            }));
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            if (_selectedDeviceStore.SelectedDevice == null)
            {
                EnableTabs = false;
                return;
            }

            EnableTabs = true;

            _availableTabs.Clear();
            _availableTabs.Add("LinkProperties");
            _availableTabs.Add("ClockPinControl");
            _availableTabs.Add("Loopback");
            _availableTabs.Add("FrameGenChecker");
            _availableTabs.Add("LoopbackFrameGenChecker");
            _availableTabs.Add("TestModes");
            _availableTabs.Add("CableDiagnostics");
            _availableTabs.Add("RegisterAccess");

            _isADIN1300CableDiagAvailable = false;
            _isADIN1100CableDiagAvailable = false;

            if (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1300
             || _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1200)
            {
                _isADIN1300CableDiagAvailable = _selectedDeviceStore.SelectedDevice.IsADIN1300CableDiagAvailable;
                _availableTabs.Remove("LoopbackFrameGenChecker");
            }

            if (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100
              || _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100_S1
              || _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1110
              || _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN2111)
            {
                _isADIN1100CableDiagAvailable = _selectedDeviceStore.SelectedDevice.IsADIN1100CableDiagAvailable;
                _availableTabs.Remove("ClockPinControl");
                _availableTabs.Remove("LoopbackFrameGenChecker");
            }

            if (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1320)
            {
                _availableTabs.Remove("LinkProperties");
                _availableTabs.Remove("ClockPinControl");
                _availableTabs.Remove("Loopback");
                _availableTabs.Remove("FrameGenChecker");
                _availableTabs.Remove("TestModes");
                _availableTabs.Remove("CableDiagnostics");
            }

            if (!(_availableTabs.Contains(_selectedTab)))
            {
                _selectedTab = _availableTabs[0];
                UpdateSelectedTab();
            }


            OnPropertyChanged(nameof(IsActiveLinkMonEnabled));
            OnPropertyChanged(nameof(IsADIN1100Visible));
            OnPropertyChanged(nameof(IsADIN1300Visible));
            OnPropertyChanged(nameof(IsLinkPropVisible));
            OnPropertyChanged(nameof(IsClkPinControlVisible));
            OnPropertyChanged(nameof(IsLoopbackVisible));
            OnPropertyChanged(nameof(IsFrameGenVisible));
            OnPropertyChanged(nameof(IsLoopbackFrameGenVisible));
            OnPropertyChanged(nameof(IsTestModeVisible));
            OnPropertyChanged(nameof(IsCableDiagVisible));
            OnPropertyChanged(nameof(IsBusy));
        }

        private bool CheckGigabitBoard(BoardType boardType = BoardType.ADIN1300)
        {
            bool result = false;

            if (boardType == BoardType.ADIN1300
             || boardType == BoardType.ADIN1200)
                result = true;

            return result;
        }
    }
}