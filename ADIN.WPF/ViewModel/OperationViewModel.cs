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
using System.Windows;

namespace ADIN.WPF.ViewModel
{
    public class OperationViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly SelectedDeviceStore _selectedDeviceStore;

        private bool _enableTabs = true;
        private bool _isAdin2111 = false;
        private bool _isCableDiagSelected;
        private bool _isCableDiagVisible = true;
        private bool _isTestModeSelected = false;

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
            FrameGenCheckerVM = new FrameGenCheckerViewModel(_selectedDeviceStore, mainLock, ftdiService);
            ClockPinControlVM = new ClockPinControlViewModel(_navigationStore, _selectedDeviceStore);
            TestModeVM = new TestModeViewModel(_selectedDeviceStore);
            DeviceStatusVM = new DeviceStatusViewModel(selectedDeviceStore, ftdiService, mainLock);
            RegisterAccessVM = new RegisterAccessViewModel(_selectedDeviceStore, navigationStore);
            RunCableDiagnosticVM = new RunCableDiagViewModel(_selectedDeviceStore, mainLock);
            TDRVM = new TimeDomainReflectometryViewModel(_selectedDeviceStore, mainLock);
            StatusStripVM = new StatusStripViewModel(_selectedDeviceStore, scriptService);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.OnGoingCalibrationStatusChanged += _selectedDeviceStore_OnGoingCalibrationStatusChanged;
            DeviceListingVM.HideCableDiagChanged += DeviceListingVM_HideCableDiagChanged;
        }

        public ClockPinControlViewModel ClockPinControlVM { get; set; }

        public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;

        public DeviceListingViewModel DeviceListingVM { get; }

        public DeviceStatusViewModel DeviceStatusVM { get; set; }

        public bool EnableTabs
        {
            get { return _enableTabs; }

            set
            {
                _enableTabs = value;
                OnPropertyChanged(nameof(EnableTabs));
            }
        }

        public ExtraCommandsViewModel ExtraCommandsVM { get; set; }

        public FrameGenCheckerViewModel FrameGenCheckerVM { get; set; }

        public bool IsActiveLinkMonEnabled { get; } = Properties.Settings.Default.ActiveLinkMon;

        public bool IsADIN1100Visible => !CheckGigabitBoard(_selectedDeviceStore.SelectedDevice?.DeviceType == null ? BoardType.ADIN1300 : BoardType.ADIN1100);

        public bool IsADIN1300Visible => CheckGigabitBoard(_selectedDeviceStore.SelectedDevice?.DeviceType == null ? BoardType.ADIN1300 : BoardType.ADIN1100);

        public bool IsADIN2111
        {
            get { return _isAdin2111; }

            set
            {
                _isAdin2111 = value;
                OnPropertyChanged(nameof(IsADIN2111));
            }
        }

        public bool IsCableDiagSelected
        {
            get { return _isCableDiagSelected; }

            set
            {
                _isCableDiagSelected = value;
                OnPropertyChanged(nameof(IsCableDiagSelected));
            }
        }

        public bool IsCableDiagVisible
        {
            get { return _isCableDiagVisible; }

            set
            {
                _isCableDiagVisible = value;
                OnPropertyChanged(nameof(IsCableDiagVisible));
            }
        }

        public bool IsTestModeSelected
        {
            get { return _isTestModeSelected; }

            set
            {
                _isTestModeSelected = value;
                OnPropertyChanged(nameof(IsTestModeSelected));
            }
        }

        public bool IsClkPinControlVisible => _selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1300 || _selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1200;

        public LinkPropertiesViewModel LinkPropertiesVM { get; set; }

        public LogActivityViewModel LogActivityVM { get; set; }

        public LoopbackViewModel LoopbackVM { get; set; }

        public MenuItemViewModel MenuItemVM { get; set; }

        public RegisterAccessViewModel RegisterAccessVM { get; set; }

        public RegisterListingViewModel RegisterListingVM { get; set; }

        public StatusStripViewModel StatusStripVM { get; set; }

        public TimeDomainReflectometryViewModel TDRVM { get; set; }

        public TestModeViewModel TestModeVM { get; set; }

        public RunCableDiagViewModel RunCableDiagnosticVM { get; set; }

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

            if (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1300
             || _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1200)
            {
                IsCableDiagVisible = _selectedDeviceStore.SelectedDevice.IsADIN1300CableDiagAvailable;
            }

            if (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100
              || _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100_S1
              || _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1110
              || _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN2111)
            {
                IsCableDiagVisible = _selectedDeviceStore.SelectedDevice.IsADIN1100CableDiagAvailable;
            }

            if ((_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN2111)
             || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1110))
            {
                IsADIN2111 = true;
                IsCableDiagSelected = true;
            }
            else
                IsADIN2111 = false;

            OnPropertyChanged(nameof(IsActiveLinkMonEnabled));
            OnPropertyChanged(nameof(IsADIN1100Visible));
            OnPropertyChanged(nameof(IsADIN1300Visible));
            OnPropertyChanged(nameof(IsClkPinControlVisible));
        }

        private bool CheckGigabitBoard(BoardType boardType = BoardType.ADIN1300)
        {
            bool result = false;

            if (boardType == BoardType.ADIN1300
             || boardType == BoardType.ADIN1200)
                result = true;

            return result;
        }

        private void DeviceListingVM_HideCableDiagChanged(bool obj)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            IsCableDiagVisible = !obj;
            //IsTestModeSelected = obj;
        }
    }
}