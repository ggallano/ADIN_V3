using ADIN.Device.Models;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class SoftwarePowerDownCommand : CommandBase
    {
        private ExtraCommandsViewModel _extraCommandsViewModel;
        private SelectedDeviceStore _selectedDeviceStore;

        public SoftwarePowerDownCommand(ExtraCommandsViewModel extraCommandsViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _extraCommandsViewModel = extraCommandsViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _extraCommandsViewModel.PropertyChanged += _extraCommandsViewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            var result = _selectedDeviceStore.SelectedDevice.FwAPI.GetPhyState() == EthPhyState.Powerdown ? true : false;
            _selectedDeviceStore.SelectedDevice.FwAPI.SoftwarePowerdown(!result);
        }

        private void _extraCommandsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}