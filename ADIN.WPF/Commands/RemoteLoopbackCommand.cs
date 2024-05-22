using ADIN.Device.Models;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class RemoteLoopbackCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private FrameGenCheckerViewModel _viewModel;

        public RemoteLoopbackCommand(FrameGenCheckerViewModel viewModel, SelectedDeviceStore selectedDeviceStore)
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
            LoopbackListingModel remoteLoopback = new LoopbackListingModel() { EnumLoopbackType = LoopBackMode.MacRemote };
            _selectedDeviceStore.SelectedDevice.FwAPI.SetLoopbackSetting(remoteLoopback);
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}