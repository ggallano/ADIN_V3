// <copyright file="ResetCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Avalonia.Stores;
using ADIN.Avalonia.ViewModels;
using ADIN.Device.Models;
using System;

namespace ADIN.Avalonia.Commands
{
    public class ResetCommand : CommandBase
    {
        private ExtraCommandsViewModel _extraCommandsViewModel;
        private SelectedDeviceStore _selectedDeviceStore;

        public ResetCommand(ExtraCommandsViewModel extraCommandsViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _extraCommandsViewModel = extraCommandsViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _extraCommandsViewModel.PropertyChanged += _extraCommandsViewModel_PropertyChanged;
            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnCanExecuteChanged();
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null ||
                !_extraCommandsViewModel.EnableButton)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            var resetType = (ResetType)Enum.Parse(typeof(ResetType), parameter?.ToString());
            _selectedDeviceStore.SelectedDevice.FwAPI.ResetPhy(resetType);
        }

        private void _extraCommandsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}