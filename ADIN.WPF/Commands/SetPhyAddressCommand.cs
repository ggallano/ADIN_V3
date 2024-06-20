using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ADIN.WPF.Commands
{
    public class SetPhyAddressCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private DeviceStatusViewModel _viewModel;


        public SetPhyAddressCommand(DeviceStatusViewModel deviceStatusViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = deviceStatusViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            //_viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            //if (_selectedDeviceStore.SelectedDevice == null)
            //    return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            _selectedDeviceStore.SelectedDevice.FwAPI.SetPhyAddress(_viewModel.PhyAddress);
        }

        private void _viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}
