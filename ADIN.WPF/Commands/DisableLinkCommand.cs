using ADIN.Device.Models;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.WPF.Commands
{
    public class DisableLinkCommand : CommandBase
    {
        private ExtraCommandsViewModel _extraCommandsViewModel;
        private SelectedDeviceStore _selectedDeviceStore;

        public DisableLinkCommand(ExtraCommandsViewModel extraCommandsViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _extraCommandsViewModel = extraCommandsViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _extraCommandsViewModel.PropertyChanged += _extraCommandsViewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null ||
                !_extraCommandsViewModel.IsPoweredUp ||
                _extraCommandsViewModel.IsOngoingCalibrationStatus)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            var result = _selectedDeviceStore.SelectedDevice.FwAPI.GetPhyState() == EthPhyState.Standby ? true : false;
            _selectedDeviceStore.SelectedDevice.FwAPI.DisableLinking(!result);
        }

        private void _extraCommandsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}
