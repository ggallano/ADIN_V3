using ADIN.WPF.Models;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class TestModeCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private TestModeViewModel _viewModel;

        public TestModeCommand(TestModeViewModel testModeViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = testModeViewModel;
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
            //var testMode = parameter as TestModeListingModel;
            //_selectedDeviceStore.SelectedDevice.FirmwareAPI.SetTestModeSetting(testMode);
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}