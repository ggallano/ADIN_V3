// <copyright file="WriteRegisterCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Services;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using Helper.ReadAccessValidate;

namespace ADIN.WPF.Commands
{
    public class WriteRegisterCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private RegisterAccessViewModel _viewModel;

        public WriteRegisterCommand(RegisterAccessViewModel registerAccessViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = registerAccessViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null ||
                _viewModel.DisableButton)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            uint register = 0;
            uint value = 0;
            bool isRegisterValid = RegisterAccessValidate.ValidateInput($"{_viewModel.WriteInput.ToLower()}", out register);
            bool isValueValid = RegisterAccessValidate.ValidateInput($"{_viewModel.WriteValue.ToLower()}", out value);

            if (!isRegisterValid)
            {
                _selectedDeviceStore.OnViewModelErrorOccured("Invalid Register Input");
                return;
            }
            if (!isValueValid)
            {
                _selectedDeviceStore.OnViewModelErrorOccured("Invalid Register Value");
                return;
            }

            if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
            {
                ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                fwAPI.RegisterWrite(register, value);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
            {
                ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                fwAPI.RegisterWrite(register, value);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
            {
                ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                fwAPI.RegisterWrite(register, value);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
            {
                ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                fwAPI.RegisterWrite(register, value);
            }
            else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
            {
                ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                fwAPI.RegisterWrite(register, value);
            }
            //_selectedDeviceStore.SelectedDevice.FwAPI.RegisterWrite(register, value);
            _selectedDeviceStore.OnViewModelErrorOccured($"[Register Write] Register Address: 0x{_viewModel.WriteInput.ToUpper()}, Value: 0x{_viewModel.WriteValue.ToUpper()}", Helper.Feedback.FeedbackType.Info);
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}