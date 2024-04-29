using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using Helper.ReadAccessValidate;
using System.Linq.Expressions;

namespace ADIN.WPF.Commands
{
    public class ClockPinControlCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private ClockPinControlViewModel _viewModel;

        public ClockPinControlCommand(ClockPinControlViewModel clockPinControlViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = clockPinControlViewModel;
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
            _selectedDeviceStore.SelectedDevice.FwAPI.SetGpClkPinControl((string)parameter);
            _viewModel.SelectedGpClk = (string)parameter;
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}