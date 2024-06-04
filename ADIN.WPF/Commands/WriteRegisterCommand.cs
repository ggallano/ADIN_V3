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
            if (_selectedDeviceStore.SelectedDevice == null)
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

            if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI fwADIN1100API)
            {
                fwADIN1100API.RegisterWrite(register, value);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI fwADIN1200API)
            {
                fwADIN1200API.RegisterWrite(register, value);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI fwADIN1300API)
            {
                fwADIN1300API.RegisterWrite(register, value);
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