// <copyright file="RunCableDiagViewModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class RunCableDiagViewModel : ViewModelBase
    {
        private string _linkStatus;
        private SelectedDeviceStore _selectedDeviceStore;
        private object _thisLock;

        public RunCableDiagViewModel(SelectedDeviceStore selectedDeviceStore, object thisLock)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _thisLock = thisLock;

            DisableLinkCommand = new DisableLinkCommand(this, _selectedDeviceStore);
            DiagnoseCommand = new DiagnoseCommand(this, _selectedDeviceStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.LinkStatusChanged += _selectedDeviceStore_LinkStatusChanged;
        }

        public string Description { get; } = "These Cable Diagnostics are only available when the PHY is in 'Stanby Mode'. Disable Linking by clicking the 'Disable Linking' button to enter 'Stanby Mode'.";

        public bool IsOngoingDiag { get; set; }

        public string BusyContent { get; set; }

        public ICommand DiagnoseCommand { get; set; }

        public ICommand DisableLinkCommand { get; set; }

        public bool IsCrossPair
        {
            get
            {
                return _selectedDeviceStore.SelectedDevice?.IsCrossPair ?? false;
            }

            set
            {
                _selectedDeviceStore.SelectedDevice.IsCrossPair = value;
                OnPropertyChanged(nameof(IsCrossPair));
            }
        }

        public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;

        public List<string> CableDiagResults
        {
            get
            {
                return _selectedDeviceStore.SelectedDevice?.CableDiagStatus;
            }

            set
            {
                //_cableDiagResults = value;
                _selectedDeviceStore.SelectedDevice.CableDiagStatus = value;
                OnPropertyChanged(nameof(CableDiagResults));
            }
        }

        public string LinkStatus
        {
            get
            {
                return _linkStatus;
            }

            set
            {
                if (value == EthPhyState.Standby.ToString())
                {
                    _linkStatus = "Enable Linking";
                }
                else
                {
                    _linkStatus = "Disable Linking";
                }

                OnPropertyChanged(nameof(LinkStatus));
            }
        }

        private void _selectedDeviceStore_LinkStatusChanged(EthPhyState linkStatus)
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                LinkStatus = linkStatus.ToString();
            }));
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(IsDeviceSelected));

            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            OnPropertyChanged(nameof(LinkStatus));
            OnPropertyChanged(nameof(CableDiagResults));
            OnPropertyChanged(nameof(IsCrossPair));
        }
    }
}
