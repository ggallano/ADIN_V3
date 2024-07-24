using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class ResetSpikeCountCommand : CommandBase
    {
        private DeviceStatusViewModel _deviceStatusViewModel;
        private SelectedDeviceStore _selectedDeviceStore;

        public ResetSpikeCountCommand(DeviceStatusViewModel deviceStatusViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            this._deviceStatusViewModel = deviceStatusViewModel;
            this._selectedDeviceStore = selectedDeviceStore;

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;

            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            _deviceStatusViewModel.SpikeCount = "0.00";
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnCanExecuteChanged();
        }
    }
}
