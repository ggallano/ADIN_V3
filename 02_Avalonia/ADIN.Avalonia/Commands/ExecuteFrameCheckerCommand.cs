// <copyright file="ExecuteFrameCheckerCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Avalonia.Stores;
using ADIN.Avalonia.ViewModels;
using ADIN.Device.Models;
using ADIN.Device.Services;
using Avalonia.Threading;

namespace ADIN.Avalonia.Commands
{
    public class ExecuteFrameCheckerCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private LoopbackFrameGenViewModel _loopbackFrameGenViewModel;
        private EthPhyState _linkStatus = EthPhyState.Powerdown;

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
            LoopbackFrameGenCheckerModel loopbackFrameGenChecker = new LoopbackFrameGenCheckerModel();

            loopbackFrameGenChecker.EnableContinuousMode = _loopbackFrameGenViewModel.EnableContinuousMode;
            loopbackFrameGenChecker.FrameBurst = _loopbackFrameGenViewModel.FrameBurst;
            loopbackFrameGenChecker.FrameLength = _loopbackFrameGenViewModel.FrameLength;
            loopbackFrameGenChecker.SelectedFrameContent = _loopbackFrameGenViewModel.SelectedFrameContent.FrameContentType;

            FrameGenCheckerModel frameGenChecker = new FrameGenCheckerModel();

            frameGenChecker.EnableContinuousMode = loopbackFrameGenChecker.EnableContinuousMode;
            frameGenChecker.FrameBurst = loopbackFrameGenChecker.FrameBurst;
            frameGenChecker.FrameLength = loopbackFrameGenChecker.FrameLength;
            frameGenChecker.SelectedFrameContent = loopbackFrameGenChecker.SelectedFrameContent;

            IFrameGenCheckerAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IFrameGenCheckerAPI;
            fwAPI.SetFrameCheckerSetting(frameGenChecker);
        }

        private async void _selectedDeviceStore_LinkStatusChanged(EthPhyState linkStatus)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _linkStatus = linkStatus;
                OnCanExecuteChanged();
            });
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}