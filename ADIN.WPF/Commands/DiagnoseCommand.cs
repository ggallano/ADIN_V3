// <copyright file="DiagnoseCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using System;
using System.Windows;

namespace ADIN.WPF.Commands
{
    public class DiagnoseCommand : CommandBase
    {
        private RunCableDiagViewModel _runCableDiagViewModel;
        private SelectedDeviceStore _selectedDeviceStore;
        private EthPhyState _linkStatus;

        public DiagnoseCommand(RunCableDiagViewModel runCableDiagViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _runCableDiagViewModel = runCableDiagViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _runCableDiagViewModel.PropertyChanged += _runCableDiagViewModel_PropertyChanged;
            _selectedDeviceStore.LinkStatusChanged += _selectedDeviceStore_LinkStatusChanged;
        }

        private void _selectedDeviceStore_LinkStatusChanged(EthPhyState linkStatus)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _linkStatus = linkStatus;
                    OnCanExecuteChanged();
                }));
        }

        public override void Execute(object parameter)
        {
            throw new NotImplementedException();
        }

        public override bool CanExecute(object parameter)
        {
            if (_linkStatus == EthPhyState.Standby)
                return base.CanExecute(parameter);

            return false;
        }

        private void _runCableDiagViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}
