// <copyright file="RemoteLoopbackCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class RemoteLoopbackCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private FrameGenCheckerViewModel _viewModel;

        public RemoteLoopbackCommand(FrameGenCheckerViewModel viewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = viewModel;
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
            LoopbackModel remoteLoopback = new LoopbackModel() { EnumLoopbackType = LoopBackMode.MacRemote };
#if !DISABLE_T1L
            if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
            {
                ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                fwAPI.SetLoopbackSetting(remoteLoopback, false, false);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
            {
                ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                fwAPI.SetLoopbackSetting(remoteLoopback, false, false);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
            {
                ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                fwAPI.SetLoopbackSetting(remoteLoopback, false, false);
            }
            else { } //Do nothing
#endif
#if !DISABLE_TSN
            if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
            {
                ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                fwAPI.SetLoopbackSetting(remoteLoopback, false, false);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)
            {
                ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                fwAPI.SetLoopbackSetting(remoteLoopback, false, false);
            }
            else { } //Do nothing
#endif
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}