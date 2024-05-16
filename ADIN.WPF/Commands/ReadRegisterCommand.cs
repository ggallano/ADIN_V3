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
            if (_selectedDeviceStore.SelectedDevice == null)
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

            _viewModel.ReadOutput = _selectedDeviceStore.SelectedDevice.FwAPI.RegisterRead(value);
            _selectedDeviceStore.OnViewModelErrorOccured($"[Register Read] Register Address: 0x{_viewModel.ReadInput.ToLower()}, Value: 0x{_viewModel.ReadOutput}", Helper.Feedback.FeedbackType.Info);
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}