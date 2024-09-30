// <copyright file="ResetSlicerErrorCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Services;
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
            if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
            {
                var fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                fwAPI.ResetMaxSlicer();
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
            {
                var fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                fwAPI.ResetMaxSlicer();
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
            {
                var fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                fwAPI.ResetMaxSlicer();
            }
            else
            {
                // Do nothing
            }
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnCanExecuteChanged();
        }
    }
}
