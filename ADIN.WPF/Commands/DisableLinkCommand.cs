// <copyright file="DisableLinkCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class DisableLinkCommand : CommandBase
    {
        private ExtraCommandsViewModel _extraCommandsViewModel;
        private EthPhyState _phyState = EthPhyState.Standby;
        private RunCableDiagViewModel _runCableDiagViewModel;
        private SelectedDeviceStore _selectedDeviceStore;

        public DisableLinkCommand(ExtraCommandsViewModel extraCommandsViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _extraCommandsViewModel = extraCommandsViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _extraCommandsViewModel.PropertyChanged += _extraCommandsViewModel_PropertyChanged;
            _selectedDeviceStore.LinkStatusChanged += _selectedDeviceStore_LinkStatusChanged;
            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        public DisableLinkCommand(RunCableDiagViewModel runCableDiagViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _runCableDiagViewModel = runCableDiagViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _runCableDiagViewModel.PropertyChanged += _runCableDiagViewModel_PropertyChanged;
            _selectedDeviceStore.LinkStatusChanged += _selectedDeviceStore_LinkStatusChanged;
            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        private void _runCableDiagViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;

            if (_phyState == EthPhyState.Powerdown)
                return false;

            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            var result = _selectedDeviceStore.SelectedDevice.FwAPI.GetPhyState() == EthPhyState.Standby ? true : false;
            _selectedDeviceStore.SelectedDevice.FwAPI.DisableLinking(!result);
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
            OnCanExecuteChanged();
        }
    }
}
