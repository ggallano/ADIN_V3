// <copyright file="DiagnoseCommand.cs" company="Analog Devices Inc.">
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
    public class DiagnoseCommand : CommandBase
    {
        private EthPhyState _linkStatus;
        private RunCableDiagViewModel _runCableDiagViewModel;
        private SelectedDeviceStore _selectedDeviceStore;
        private string busyContent = "On-going Cable Diagnostic";

        public DiagnoseCommand(RunCableDiagViewModel runCableDiagViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _runCableDiagViewModel = runCableDiagViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _runCableDiagViewModel.PropertyChanged += _runCableDiagViewModel_PropertyChanged;
            _selectedDeviceStore.LinkStatusChanged += _selectedDeviceStore_LinkStatusChanged;
            _selectedDeviceStore.GigabitCableDiagCompleted += _selectedDeviceStore_GigabitCableDiagCompleted;
        }

        private void _selectedDeviceStore_GigabitCableDiagCompleted(System.Collections.Generic.List<string> results)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => 
            {
                _runCableDiagViewModel.CableDiagResults = results;
            }));
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice != null
                && _linkStatus == EthPhyState.Standby)
                return base.CanExecute(parameter);

            return false;
        }

        public override void Execute(object parameter)
        {

            try
            {
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    ExecuteCableDiag(_selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI);
                else
                    ExecuteCableDiag(_selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI);

            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                _runCableDiagViewModel.IsOngoingDiag = false;
            }
        }

        private void _runCableDiagViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }

        private void _selectedDeviceStore_LinkStatusChanged(EthPhyState linkStatus)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _linkStatus = linkStatus;
                    OnCanExecuteChanged();
                }));
        }

        private void ExecuteCableDiag(ADIN1300FirmwareAPI fwAPI)
        {
            fwAPI.RunCableDiagnostics(_runCableDiagViewModel.IsCrossPair);
            _runCableDiagViewModel.IsOngoingDiag = true;

            _runCableDiagViewModel.BusyContent = busyContent;
        }

        private void ExecuteCableDiag(ADIN1200FirmwareAPI fwAPI)
        {
            fwAPI.RunCableDiagnostics(_runCableDiagViewModel.IsCrossPair);
            _runCableDiagViewModel.IsOngoingDiag = true;

            _runCableDiagViewModel.BusyContent = busyContent;
        }
    }
}
