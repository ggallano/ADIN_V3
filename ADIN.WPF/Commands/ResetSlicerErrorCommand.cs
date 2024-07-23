using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class ResetSlicerErrorCommand : CommandBase
    {
        private DeviceStatusViewModel _deviceStatusViewModel;
        private SelectedDeviceStore _selectedDeviceStore;

        public ResetSlicerErrorCommand(DeviceStatusViewModel deviceStatusViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            this._deviceStatusViewModel = deviceStatusViewModel;
            this._selectedDeviceStore = selectedDeviceStore;
        }

        public override void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
