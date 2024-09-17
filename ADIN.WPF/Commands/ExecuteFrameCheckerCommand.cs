// <copyright file="ExecuteFrameCheckerCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using System;
using System.Windows;

namespace ADIN.WPF.Commands
{
    public class ExecuteFrameCheckerCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private FrameGenCheckerViewModel _framgeGenCheckerViewModel;
        private LoopbackFrameGenViewModel _loopbackFrameGenViewModel;
        private EthPhyState _linkStatus = EthPhyState.Powerdown;

        public ExecuteFrameCheckerCommand(FrameGenCheckerViewModel viewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _framgeGenCheckerViewModel = viewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _framgeGenCheckerViewModel.PropertyChanged += _viewModel_PropertyChanged;
            _selectedDeviceStore.LinkStatusChanged += _selectedDeviceStore_LinkStatusChanged;
        }

        public ExecuteFrameCheckerCommand(LoopbackFrameGenViewModel viewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _loopbackFrameGenViewModel = viewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _loopbackFrameGenViewModel.PropertyChanged += _viewModel_PropertyChanged;
            _selectedDeviceStore.LinkStatusChanged += _selectedDeviceStore_LinkStatusChanged;
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;

            if (_linkStatus != EthPhyState.LinkUp)
                return false;

            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            if (_framgeGenCheckerViewModel != null)
            {
                FrameGenCheckerModel frameGenChecker = new FrameGenCheckerModel();

                frameGenChecker.EnableContinuousMode = _framgeGenCheckerViewModel.EnableContinuousMode;
                frameGenChecker.FrameBurst = _framgeGenCheckerViewModel.FrameBurst_Value;
                frameGenChecker.FrameLength = _framgeGenCheckerViewModel.FrameLength_Value;
                frameGenChecker.SelectedFrameContent = _framgeGenCheckerViewModel.SelectedFrameContent.FrameContentType;
                frameGenChecker.EnableMacAddress = _framgeGenCheckerViewModel.EnableMacAddress;
                frameGenChecker.SrcMacAddress = _framgeGenCheckerViewModel.SrcMacAddress;
                frameGenChecker.DestMacAddress = _framgeGenCheckerViewModel.DestMacAddress;
                frameGenChecker.SrcOctet = _framgeGenCheckerViewModel.SrcOctet;
                frameGenChecker.DestOctet = _framgeGenCheckerViewModel.DestOctet;

#if !DISABLE_T1L
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                {
                    ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                    fwAPI.SetFrameCheckerSetting(frameGenChecker);
                }
                else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                {
                    ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                    fwAPI.SetFrameCheckerSetting(frameGenChecker);
                }
                else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                {
                    ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                    fwAPI.SetFrameCheckerSetting(frameGenChecker);
                }
                else { } //Do nothing
#endif
#if !DISABLE_TSN
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.SetFrameCheckerSetting(frameGenChecker);
                }
                else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.SetFrameCheckerSetting(frameGenChecker);
                }
                else { } //Do nothing
#endif
            }

            else
            {
                LoopbackFrameGenCheckerModel loopbackFrameGenChecker = new LoopbackFrameGenCheckerModel();

                loopbackFrameGenChecker.EnableContinuousMode = _loopbackFrameGenViewModel.EnableContinuousMode;
                loopbackFrameGenChecker.FrameBurst = _loopbackFrameGenViewModel.FrameBurst_Value;
                loopbackFrameGenChecker.FrameLength = _loopbackFrameGenViewModel.FrameLength_Value;
                loopbackFrameGenChecker.SelectedFrameContent = _loopbackFrameGenViewModel.SelectedFrameContent.FrameContentType;

                FrameGenCheckerModel frameGenChecker = new FrameGenCheckerModel();

                frameGenChecker.EnableContinuousMode = loopbackFrameGenChecker.EnableContinuousMode;
                frameGenChecker.FrameBurst = loopbackFrameGenChecker.FrameBurst;
                frameGenChecker.FrameLength = loopbackFrameGenChecker.FrameLength;
                frameGenChecker.SelectedFrameContent = loopbackFrameGenChecker.SelectedFrameContent;

                ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                fwAPI.SetFrameCheckerSetting(frameGenChecker);
            }
        }

        private void _selectedDeviceStore_LinkStatusChanged(EthPhyState linkStatus)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _linkStatus = linkStatus;
                OnCanExecuteChanged();
            }));
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}