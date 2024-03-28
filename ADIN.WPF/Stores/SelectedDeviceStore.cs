using ADIN.Device.Models;
using ADIN.WPF.Models;
using Helper.Feedback;
using System;
using System.Windows;

namespace ADIN.WPF.Stores
{
    public class SelectedDeviceStore
    {
        private ADINDevice _selectedDevice;

        public ADINDevice SelectedDevice
        {
            get { return _selectedDevice; }
            set 
            { 
                _selectedDevice = value;
                SelectedDeviceChanged?.Invoke();
            }
        }

        public event Action SelectedDeviceChanged;
    }
}