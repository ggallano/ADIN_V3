using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class ScriptApplyCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private StatusStripViewModel _viewModel;

        public ScriptApplyCommand(StatusStripViewModel statusStripViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = statusStripViewModel;
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
            _selectedDeviceStore.SelectedDevice.FirmwareAPI.ExecuteSript(_viewModel.SelectedScript);
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}