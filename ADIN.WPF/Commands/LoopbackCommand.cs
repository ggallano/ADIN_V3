using ADIN.Device.Models;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using Helper.ReadAccessValidate;
using System.ComponentModel;
using System.Linq.Expressions;

namespace ADIN.WPF.Commands
{
    public class LoopbackCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private LoopbackViewModel _viewModel;

        public LoopbackCommand(LoopbackViewModel loopbackViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = loopbackViewModel;
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
            LoopbackListingModel loopback = new LoopbackListingModel();
            loopback.EnumLoopbackType = (LoopBackMode)parameter;

            switch (loopback.EnumLoopbackType)
            {
                case LoopBackMode.OFF:
                    _viewModel.SelectedLoopback = _viewModel.Loopbacks[0];
                    break;

                case LoopBackMode.Digital:
                    _viewModel.SelectedLoopback = _viewModel.Loopbacks[1];
                    break;

                case LoopBackMode.LineDriver:
                    _viewModel.SelectedLoopback = _viewModel.Loopbacks[2];
                    break;

                case LoopBackMode.ExtCable:
                    _viewModel.SelectedLoopback = _viewModel.Loopbacks[3];
                    break;

                case LoopBackMode.MacRemote:
                    _viewModel.SelectedLoopback = _viewModel.Loopbacks[4];
                    break;

                default:
                    break;
            }
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}