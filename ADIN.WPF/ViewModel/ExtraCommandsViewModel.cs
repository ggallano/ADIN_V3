// <copyright file="ExtraCommandsViewModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Windows;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class ExtraCommandsViewModel : ViewModelBase
    {
        private bool _enableButton = true;
        private IFTDIServices _ftdiService;
        private string _linkStatus = "Disable Linking";
        private string _powerDownStatus = "Software Power Down";
        private SelectedDeviceStore _selectedDeviceStore;

        public ExtraCommandsViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;

            SoftwarePowerDownCommand = new SoftwarePowerDownCommand(this, selectedDeviceStore);
            AutoNegCommand = new AutoNegCommand(this, selectedDeviceStore);
            DisableLinkCommand = new DisableLinkCommand(this, selectedDeviceStore);
            SubSysResetCommand = new ResetCommand(this, selectedDeviceStore);
            SubSysPinResetCommand = new ResetCommand(this, selectedDeviceStore);
            PhyResetCommand = new ResetCommand(this, selectedDeviceStore);
            RegisterActionCommand = new RegisterActionCommand(this, selectedDeviceStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.SoftwarePowerDownChanged += _selectedDeviceStore_PowerDownStateStatusChanged;
            _selectedDeviceStore.LinkStatusChanged += _selectedDeviceStore_LinkStateStatusChanged;
            _selectedDeviceStore.OnGoingCalibrationStatusChanged += _selectedDeviceStore_OnGoingCalibrationStatusChanged;
        }

        public ICommand AutoNegCommand { get; set; }

        public ICommand DisableLinkCommand { get; set; }

        public bool EnableButton
        {
            get
            {
                return _enableButton;
            }

            set
            {
                _enableButton = value;
                OnPropertyChanged(nameof(EnableButton));
            }
        }

#if !DISABLE_TSN && !DISABLE_T1L

        public bool IsT1LBoard
        {
            get
            {
                return ((_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100)
                    || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100_S1)
                    || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1110)
                    || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN2111)) == true;
            }
        }

        public bool IsGigabitBoard => !IsT1LBoard;

#elif !DISABLE_TSN
        public bool IsGigabitBoard { get; } = true;
        public bool IsT1LBoard { get; } = false;
#elif !DISABLE_T1L
        public bool IsGigabitBoard { get; } = false;

        public bool IsT1LBoard { get; } = true;
#endif

        public bool IsPortNumVisible
        {
            get { return _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN2111; }
        }

        public bool IsResetButtonVisible
        {
            get
            {
                if (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1110
                    || _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN2111)
                    return false;
                else
                    return true;
            }
        }

        public string LinkStatus
        {
            get
            {
                return _linkStatus;
            }

            set
            {
                if (value == EthPhyState.Standby.ToString())
                {
                    _linkStatus = "Enable Linking";
                }
                else
                {
                    _linkStatus = "Disable Linking";
                }

                OnPropertyChanged(nameof(LinkStatus));
            }
        }

        public ICommand PhyResetCommand { get; set; }

        public string PowerDownStatus
        {
            get
            {
                return _powerDownStatus;
            }

            set
            {
                if (_powerDownStatus != value)
                {
                    _powerDownStatus = value;
                    OnPropertyChanged(nameof(PowerDownStatus));
                }
            }
        }

        public ICommand RegisterActionCommand { get; set; }

        public ICommand SoftwarePowerDownCommand { get; set; }

        public ICommand SubSysPinResetCommand { get; set; }

        public ICommand SubSysResetCommand { get; set; }

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.SoftwarePowerDownChanged -= _selectedDeviceStore_PowerDownStateStatusChanged;
            _selectedDeviceStore.LinkStatusChanged -= _selectedDeviceStore_LinkStateStatusChanged;
            _selectedDeviceStore.OnGoingCalibrationStatusChanged -= _selectedDeviceStore_OnGoingCalibrationStatusChanged;

            base.Dispose();
        }

        private void _selectedDeviceStore_LinkStateStatusChanged(EthPhyState linkStatus)
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                LinkStatus = linkStatus.ToString();
            }));
        }

        private void _selectedDeviceStore_OnGoingCalibrationStatusChanged(bool onGoingCalibrationStatus)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                EnableButton = !onGoingCalibrationStatus;
            }));
        }

        private void _selectedDeviceStore_PowerDownStateStatusChanged(string powerDownStatus)
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                PowerDownStatus = powerDownStatus;
            }));
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            OnPropertyChanged(nameof(IsGigabitBoard));
            OnPropertyChanged(nameof(IsT1LBoard));

            OnPropertyChanged(nameof(PowerDownStatus));
            OnPropertyChanged(nameof(LinkStatus));
            OnPropertyChanged(nameof(IsPortNumVisible));
            OnPropertyChanged(nameof(IsResetButtonVisible));
            OnPropertyChanged(nameof(EnableButton));
        }
    }
}