// <copyright file="AutoNegCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Avalonia.Stores;
using ADIN.Avalonia.ViewModels;
using ADIN.Device.Models;

namespace ADIN.Avalonia.Commands
{
    public class AutoNegCommand : CommandBase
    {
        private ExtraCommandsViewModel _extraCommandsViewModel;
        private EthPhyState _phyState = EthPhyState.Standby;
        private SelectedDeviceStore _selectedDeviceStore;
        private bool _isOngoingCalibration;

        public AutoNegCommand(ExtraCommandsViewModel extraCommandsViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _extraCommandsViewModel = extraCommandsViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _extraCommandsViewModel.PropertyChanged += _extraCommandsViewModel_PropertyChanged;
            _selectedDeviceStore.LinkStatusChanged += _selectedDeviceStore_LinkStatusChanged;
            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.OnGoingCalibrationStatusChanged += _selectedDeviceStore_OnGoingCalibrationStatusChanged;
        }

        private void _selectedDeviceStore_OnGoingCalibrationStatusChanged(bool status)
        {
            _isOngoingCalibration = status;
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;

            if (_phyState == EthPhyState.Powerdown || _isOngoingCalibration)
                return false;

            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            _selectedDeviceStore.SelectedDevice.FwAPI.RestartAutoNegotiation();
        }

        private void _extraCommandsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }

        private void _selectedDeviceStore_LinkStatusChanged(EthPhyState phyState)
        {
            _phyState = phyState;
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            if (_selectedDeviceStore.SelectedDevice != null)
                return;

            OnCanExecuteChanged();
        }
    }
}