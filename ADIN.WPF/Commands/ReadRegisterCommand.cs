using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using Helper.ReadAccessValidate;

namespace ADIN.WPF.Commands
{
    public class ReadRegisterCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private RegisterAccessViewModel _viewModel;

        public ReadRegisterCommand(RegisterAccessViewModel registerAccessViewModel, SelectedDeviceStore selectedDeviceStore)
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
            uint value = 0;
            if (_viewModel.ReadInput == string.Empty || _viewModel.ReadInput == null)
            {
                _selectedDeviceStore.OnViewModelErrorOccured("Invalid Register Input");
                return;
            }

            bool isValid = RegisterAccessValidate.ValidateInput($"{_viewModel.ReadInput.ToLower()}", out value);

            if (!isValid)
            {
                _selectedDeviceStore.OnViewModelErrorOccured("Invalid Register Input");
                return;
            }

#if !DISABLE_T1L
            if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
            {
                ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                _viewModel.ReadOutput = fwAPI.RegisterRead(value);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
            {
                ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                _viewModel.ReadOutput = fwAPI.RegisterRead(value);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
            {
                ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                _viewModel.ReadOutput = fwAPI.RegisterRead(value);
            }
            else { } //Do nothing
#endif
#if !DISABLE_TSN
            if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
            {
                ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                _viewModel.ReadOutput = fwAPI.RegisterRead(value);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)
            {
                ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                _viewModel.ReadOutput = fwAPI.RegisterRead(value);
            }
            else { } //Do nothing
#endif
            _selectedDeviceStore.OnViewModelErrorOccured($"[Register Read] Register Address: 0x{_viewModel.ReadInput.ToUpper()}, Value: 0x{_viewModel.ReadOutput}", Helper.Feedback.FeedbackType.Info);
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}