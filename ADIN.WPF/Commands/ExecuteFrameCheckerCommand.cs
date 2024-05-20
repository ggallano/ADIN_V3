using ADIN.Device.Models;
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
            frameGenChecker.FrameBurst = _viewModel.FrameBurst;
            frameGenChecker.FrameLength = _viewModel.FrameLength;
            frameGenChecker.SelectedFrameContent = _viewModel.SelectedFrameContent.FrameContentType;
            frameGenChecker.EnableMacAddress = _viewModel.EnableMacAddress;
            frameGenChecker.SrcMacAddress = _viewModel.SrcMacAddress;
            frameGenChecker.DestMacAddress = _viewModel.DestMacAddress;
            frameGenChecker.SrcOctet = _viewModel.SrcOctet;
            frameGenChecker.DestOctet = _viewModel.DestOctet;

            _selectedDeviceStore.SelectedDevice.FwAPI.SetFrameCheckerSetting(frameGenChecker);
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}