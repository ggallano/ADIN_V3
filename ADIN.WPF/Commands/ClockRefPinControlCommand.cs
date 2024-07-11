// <copyright file="ClockRefPinControlCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Services;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class ClockRefPinControlCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private ClockPinControlViewModel _viewModel;

        public ClockRefPinControlCommand(ClockPinControlViewModel clockPinControlViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = clockPinControlViewModel;
            _selectedDeviceStore = selectedDeviceStore;
            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
            {
                ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                fwAPI.SetClk25RefPinControl((string)parameter);
            }
            else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
            {
                ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                fwAPI.SetClk25RefPinControl((string)parameter);
            }

            _viewModel.SelectedClk25RefPnCtrl = (string)parameter;
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}