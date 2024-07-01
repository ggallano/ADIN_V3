using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class ExecuteFrameCheckerCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private FrameGenCheckerViewModel _viewModel;

        public ExecuteFrameCheckerCommand(FrameGenCheckerViewModel viewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = viewModel;
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
            FrameGenCheckerModel frameGenChecker = new FrameGenCheckerModel();

            frameGenChecker.EnableContinuousMode = _viewModel.EnableContinuousMode;
            frameGenChecker.FrameBurst = _viewModel.FrameBurst_Value;
            frameGenChecker.FrameLength = _viewModel.FrameLength_Value;
            frameGenChecker.SelectedFrameContent = _viewModel.SelectedFrameContent.FrameContentType;
            frameGenChecker.EnableMacAddress = _viewModel.EnableMacAddress;
            frameGenChecker.SrcMacAddress = _viewModel.SrcMacAddress;
            frameGenChecker.DestMacAddress = _viewModel.DestMacAddress;
            frameGenChecker.SrcOctet = _viewModel.SrcOctet;
            frameGenChecker.DestOctet = _viewModel.DestOctet;

#if !DISABLE_T1L
            if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
            {
                ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                fwAPI.SetFrameCheckerSetting(frameGenChecker);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
            {
                ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                fwAPI.SetFrameCheckerSetting(frameGenChecker);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
            {
                ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                fwAPI.SetFrameCheckerSetting(frameGenChecker);
            }
            else { } //Do nothing
#endif
#if !DISABLE_TSN
            if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
            {
                ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                fwAPI.SetFrameCheckerSetting(frameGenChecker);
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)
            {
                ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                fwAPI.SetFrameCheckerSetting(frameGenChecker);
            }
            else { } //Do nothing
#endif
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}