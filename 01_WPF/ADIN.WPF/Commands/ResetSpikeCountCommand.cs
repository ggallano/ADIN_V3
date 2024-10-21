using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using ADIN.Device.Services;

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
            if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
            {
                var fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                fwAPI.ResetSpikeCount();
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
            {
                var fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                fwAPI.ResetSpikeCount();
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
            {
                var fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                fwAPI.ResetSpikeCount();
            }
            else
            {
                // Do nothing
            }
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnCanExecuteChanged();
        }
    }
}
