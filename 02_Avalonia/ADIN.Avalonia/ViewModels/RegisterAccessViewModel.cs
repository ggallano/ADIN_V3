// <copyright file="RegisterAccessViewModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Avalonia.Commands;
using ADIN.Avalonia.Stores;
using Avalonia.Threading;
using System.Windows.Input;

namespace ADIN.Avalonia.ViewModels
{
    public class RegisterAccessViewModel : ViewModelBase
    {
        protected SelectedDeviceStore _selectedDeviceStore;
        private bool _isEnable;
        private string _readInput = string.Empty;
        private string _readOutput = string.Empty;
        private string _writeInput = string.Empty;
        private string _writeValue = string.Empty;
        private bool _disableButton = false;

        public RegisterAccessViewModel(SelectedDeviceStore selectedDeviceStore, NavigationStore navigationStore)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _isEnable = false;

            ReadRegisterCommand = new ReadRegisterCommand(this, selectedDeviceStore);
            WriteRegisterCommand = new WriteRegisterCommand(this, selectedDeviceStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.OnGoingCalibrationStatusChanged += _selectedDeviceStore_OnGoingCalibrationStatusChanged;
        }

        public bool IsEnable
        {
            get
            {
                return _isEnable;
            }

            set
            {
                _isEnable = value;
                OnPropertyChanged(nameof(IsEnable));
            }
        }

        public string ReadInput
        {
            get
            {
                return _readInput;
            }

            set
            {
                _readInput = value;
                OnPropertyChanged(nameof(ReadInput));
            }
        }

        public string ReadOutput
        {
            get
            {
                return _readOutput;
            }

            set
            {
                _readOutput = value;
                OnPropertyChanged(nameof(ReadOutput));
            }
        }

        public ICommand ReadRegisterCommand { get; set; }

        public string WriteInput
        {
            get { return _writeInput; }
            set { _writeInput = value; }
        }

        public ICommand WriteRegisterCommand { get; set; }

        public string WriteValue
        {
            get { return _writeValue; }
            set { _writeValue = value; }
        }

        public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;

        public bool DisableButton
        {
            get
            {
                return _disableButton;
            }

            set
            {
                _disableButton = value;
                OnPropertyChanged(nameof(DisableButton));
            }
        }

        private void _selectedDeviceStore_OnGoingCalibrationStatusChanged(bool onGoingCalibrationStatus)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            Dispatcher.UIThread.Post(() =>
            {
                DisableButton = onGoingCalibrationStatus;
            });
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(IsDeviceSelected));

            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            IsEnable = true;
        }
    }
}