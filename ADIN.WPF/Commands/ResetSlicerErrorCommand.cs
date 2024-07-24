// <copyright file="ResetSlicerErrorCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class ResetSlicerErrorCommand : CommandBase
    {
        private DeviceStatusViewModel _deviceStatusViewModel;
        private SelectedDeviceStore _selectedDeviceStore;

        public ResetSlicerErrorCommand(DeviceStatusViewModel deviceStatusViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            this._deviceStatusViewModel = deviceStatusViewModel;
            this._selectedDeviceStore = selectedDeviceStore;

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;

            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            _deviceStatusViewModel.MaxSlicerError = "0.00";
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnCanExecuteChanged();
        }
    }
}
