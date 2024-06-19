using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class AutoNegCommand : CommandBase
    {
        private ExtraCommandsViewModel _extraCommandsViewModel;
        private SelectedDeviceStore _selectedDeviceStore;

        public AutoNegCommand(ExtraCommandsViewModel extraCommandsViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _extraCommandsViewModel = extraCommandsViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _extraCommandsViewModel.PropertyChanged += _extraCommandsViewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null ||
                !_extraCommandsViewModel.IsPoweredUp ||
                _extraCommandsViewModel.DisableButton)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            _selectedDeviceStore.SelectedDevice.FwAPI.RestartAutoNegotiation();
        }

        private void _extraCommandsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}