using ADIN.Device.Services;
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
            if (_selectedDeviceStore.SelectedDevice == null ||
                _viewModel.SelectedTestMode == null)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            TestModeListingModel testmode = _selectedDeviceStore.SelectedDevice.TestMode.TestMode;
            uint framelength = _selectedDeviceStore.SelectedDevice.TestMode.TestModeFrameLength;

            if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
            {
                ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                fwAPI.SetTestMode(testmode, framelength);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
            {
                ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                fwAPI.SetTestMode(testmode, framelength);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
            {
                ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                fwAPI.SetTestMode(testmode, framelength);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
            {
                ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                fwAPI.SetTestMode(testmode, framelength);
            }
            else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
            {
                ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                fwAPI.SetTestMode(testmode, framelength);
            }
            //_selectedDeviceStore.SelectedDevice.FwAPI.SetTestMode(testmode, framelength);
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}